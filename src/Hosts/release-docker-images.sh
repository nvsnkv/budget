#!/usr/bin/env bash
# Builds and releases the Docker image for the Budget application (server with embedded client)
#
# This script builds the budget-server Docker image (ASP.NET Core app with the
# Angular client embedded in wwwroot), optionally tags it with version information,
# and pushes it to a Docker registry.
#
# Usage:
#   ./release-docker-images.sh                                  # auto version, build + push
#   ./release-docker-images.sh --version 1.2.3                  # specific version
#   ./release-docker-images.sh --skip-push                      # build locally only
#   ./release-docker-images.sh --skip-build --version 1.2.3     # push an existing image
#   ./release-docker-images.sh --cleanup-old-images --keep-versions 3
#
# Options:
#   --version <version>      Version tag for the image (e.g., "1.0.0", "2026.1.1").
#                            If not specified, automatically generates the next version
#                            in format Year.Month.Number (e.g., 2026.10.1, 2026.10.2)
#   --skip-build             Skip building images and only push existing ones
#   --skip-push              Build images but skip pushing to registry
#   --configure-registry     Force reconfiguration of Docker registry settings
#   --cleanup-old-images     Remove old Docker images, keeping only recent versions
#   --keep-versions <n>      Number of recent versions to keep when cleaning up (default: 5)
#   -h, --help               Show this help
#
# The registry password can also be provided via the DOCKER_REGISTRY_PASSWORD
# environment variable (useful for CI), overriding the one stored in the config file.

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG_FILE="$SCRIPT_DIR/docker-registry-config.json"

# Script configuration
VERSION=""
SKIP_BUILD=false
SKIP_PUSH=false
CONFIGURE_REGISTRY=false
CLEANUP_OLD_IMAGES=false
KEEP_VERSIONS=5

# Color output (disabled when not a terminal or NO_COLOR is set)
if [ -t 1 ] && [ -z "${NO_COLOR:-}" ]; then
    C_RESET=$'\033[0m'
    C_CYAN=$'\033[36m'
    C_GREEN=$'\033[32m'
    C_YELLOW=$'\033[33m'
    C_RED=$'\033[31m'
    C_MAGENTA=$'\033[35m'
else
    C_RESET=""
    C_CYAN=""
    C_GREEN=""
    C_YELLOW=""
    C_RED=""
    C_MAGENTA=""
fi

log_success() { printf '%s\n' "${C_GREEN}✓ $*${C_RESET}"; }
log_info()    { printf '%s\n' "${C_CYAN}ℹ $*${C_RESET}"; }
log_warn()    { printf '%s\n' "${C_YELLOW}⚠ $*${C_RESET}"; }
log_error()   { printf '%s\n' "${C_RED}✗ $*${C_RESET}"; }
log_step()    { printf '\n%s\n' "${C_MAGENTA}==> $*${C_RESET}"; }

trim() {
    printf '%s' "$1" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//'
}

usage() {
    # Print the leading comment block of this script
    sed -n '2,/^[^#]/p' "${BASH_SOURCE[0]}" | sed -e 's/^# \{0,1\}//'
}

# Reads a top-level string field from the JSON config file
config_get() {
    [ -f "$CONFIG_FILE" ] || return 1
    sed -n "s/.*\"$1\"[[:space:]]*:[[:space:]]*\"\([^\"]*\)\".*/\1/p" "$CONFIG_FILE" | head -n 1
}

# Loads registry settings into globals; succeeds when a registry is configured
load_registry_config() {
    REGISTRY="$(config_get Registry || true)"
    REGISTRY_USERNAME="$(config_get Username || true)"
    REGISTRY_PASSWORD="$(config_get Password || true)"
    REGISTRY_NAMESPACE="$(config_get Namespace || true)"
    [ -n "$REGISTRY" ]
}

json_escape() {
    printf '%s' "$1" | sed -e 's/\\/\\\\/g' -e 's/"/\\"/g'
}

save_registry_config() {
    {
        printf '{\n'
        printf '  "Registry": "%s",\n' "$(json_escape "$REGISTRY")"
        printf '  "Username": "%s",\n' "$(json_escape "$REGISTRY_USERNAME")"
        printf '  "Password": "%s",\n' "$(json_escape "$REGISTRY_PASSWORD")"
        printf '  "Namespace": "%s",\n' "$(json_escape "$REGISTRY_NAMESPACE")"
        printf '  "ConfiguredDate": "%s"\n' "$(date '+%Y-%m-%d %H:%M:%S')"
        printf '}\n'
    } > "$CONFIG_FILE"
    chmod 600 "$CONFIG_FILE"
    log_success "Registry configuration saved to: $CONFIG_FILE"
}

# Configure Docker registry (interactive)
initialize_registry_config() {
    log_step "Docker Registry Configuration"

    log_info "Please provide Docker registry information:"
    log_info "(Press Enter to skip and use local images only)"
    echo

    printf 'Docker Registry URL (e.g., docker.io, ghcr.io, registry.example.com): '
    read -r registry
    registry="$(trim "$registry")"

    if [ -z "$registry" ]; then
        log_warn "No registry configured. Images will only be built locally."
        return 1
    fi

    printf 'Registry Username: '
    read -r REGISTRY_USERNAME

    printf 'Registry Password: '
    read -rs REGISTRY_PASSWORD
    echo

    printf 'Image Namespace/Repository (e.g., mycompany/budget): '
    read -r REGISTRY_NAMESPACE
    REGISTRY_NAMESPACE="$(trim "$REGISTRY_NAMESPACE")"

    REGISTRY="$registry"
    save_registry_config
    return 0
}

# Docker login
connect_docker_registry() {
    if [ -z "$REGISTRY" ]; then
        return 1
    fi

    log_step "Logging into Docker Registry"

    local password="${DOCKER_REGISTRY_PASSWORD:-$REGISTRY_PASSWORD}"

    if ! printf '%s' "$password" | docker login "$REGISTRY" --username "$REGISTRY_USERNAME" --password-stdin; then
        log_error "Docker login failed"
        return 1
    fi

    log_success "Successfully logged into $REGISTRY"
}

# Build Docker image
build_docker_image() {
    local name="$1" dockerfile="$2" context="$3" tag="$4"

    log_step "Building $name"
    log_info "Dockerfile: $dockerfile"
    log_info "Context: $context"
    log_info "Tag: $tag"

    if ! docker build -f "$dockerfile" -t "$tag" "$context"; then
        log_error "Failed to build $name"
        return 1
    fi

    log_success "Successfully built $name"
}

# Tag and push image
publish_docker_image() {
    local local_tag="$1" remote_tag="$2" name="$3"

    log_step "Publishing $name"

    log_info "Tagging image: $local_tag -> $remote_tag"
    if ! docker tag "$local_tag" "$remote_tag"; then
        log_error "Failed to tag $name"
        return 1
    fi

    log_info "Pushing image: $remote_tag"
    if ! docker push "$remote_tag"; then
        log_error "Failed to push $name"
        return 1
    fi

    log_success "Successfully pushed $name to registry"
}

# Create git tag for release version
ensure_git_tag() {
    local repo_root="$1" version="$2"

    log_step "Creating Git tag"

    if ! git -C "$repo_root" --version >/dev/null 2>&1; then
        log_warn "Git is not available. Skipping tag creation."
        return 0
    fi

    if [ -n "$(git -C "$repo_root" tag --list "$version")" ]; then
        log_info "Git tag '$version' already exists. Skipping creation."
        return 0
    fi

    if ! git -C "$repo_root" tag -a "$version" -m "Release $version"; then
        log_error "Failed to create git tag '$version'"
        exit 1
    fi

    log_success "Created git tag '$version'"
}

# Get next version in format Year.Month.Number
get_next_version() {
    local repo_root="$1" image_name="$2"
    local current_year current_month pattern versions version max_number number

    current_year="$(date +%Y)"
    current_month=$((10#$(date +%m)))
    pattern="^${current_year}\\.${current_month}\\.[0-9]+$"

    versions=""

    # Git tags are the authoritative record of released versions
    versions+="$(git -C "$repo_root" tag --list 2>/dev/null | grep -E "$pattern" || true)"
    # Local image tags serve as an additional source
    versions+="
$(docker images --format '{{.Tag}}' "$image_name" 2>/dev/null | grep -E "$pattern" || true)"

    # Find the highest number for current year.month
    max_number=0
    for version in $versions; do
        number=$((10#${version##*.}))
        if [ "$number" -gt "$max_number" ]; then
            max_number="$number"
        fi
    done

    # Return next version
    printf '%s.%s.%s\n' "$current_year" "$current_month" "$((max_number + 1))"
}

# Lists version-tagged images for the given repository patterns as "sortkey|full-name" lines,
# deduplicated by tag (newest first after sorting)
list_versioned_images() {
    local pattern
    for pattern in "$@"; do
        docker images --format '{{.Repository}}:{{.Tag}}' "$pattern" 2>/dev/null || true
    done | awk -F':' '
        {
            tag = $NF
            if (tag ~ /^[0-9][0-9][0-9][0-9][.][0-9]+[.][0-9]+$/ && !(tag in seen)) {
                seen[tag] = 1
                split(tag, p, ".")
                printf "%012d|%s\n", p[1] * 10000 + p[2] * 100 + p[3], $0
            }
        }'
}

# Clean up old Docker images
remove_old_images() {
    local image_name="$1" registry_prefix="${2:-}" keep_count="$3"

    log_step "Cleaning up old images (keeping $keep_count most recent)"

    # Get local images (both base name and registry-prefixed if pulled)
    local patterns=("$image_name")
    if [ -n "$registry_prefix" ]; then
        patterns+=("${registry_prefix}/${image_name}")
    fi

    local list
    list="$(list_versioned_images "${patterns[@]}")"

    if [ -z "$list" ]; then
        log_info "No versioned images found to clean up."
        return 0
    fi

    local total
    total="$(printf '%s\n' "$list" | grep -c . || true)"

    if [ "$total" -le "$keep_count" ]; then
        log_info "Found $total versioned image(s). No cleanup needed (keeping $keep_count)."
        return 0
    fi

    # Sort by version (newest first) and drop everything after the kept count
    local to_remove
    to_remove="$(printf '%s\n' "$list" | sort -t'|' -k1,1 -rn | awk -F'|' -v keep="$keep_count" 'NR > keep { print $2 }')"

    local remove_count
    remove_count="$(printf '%s\n' "$to_remove" | grep -c . || true)"

    log_info "Found $total versioned image(s). Removing $remove_count old image(s)..."

    local removed=0 failed=0 image error
    while IFS= read -r image; do
        [ -n "$image" ] || continue
        log_info "Removing $image..."
        if error="$(docker rmi "$image" 2>&1)"; then
            log_success "Removed $image"
            removed=$((removed + 1))
        else
            if printf '%s' "$error" | grep -qE "image is being used|image has dependent child images"; then
                log_warn "Skipped $image (image is in use)"
            else
                log_warn "Failed to remove $image: $error"
            fi
            failed=$((failed + 1))
        fi
    done <<< "$to_remove"

    if [ "$removed" -gt 0 ]; then
        log_success "Cleanup complete. Removed $removed image(s), kept $keep_count most recent version(s)."
    fi
    if [ "$failed" -gt 0 ]; then
        log_warn "$failed image(s) could not be removed (may be in use)."
    fi
}

# Main script execution
main() {
    printf '%s\n' "${C_CYAN}╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║        Budget Application - Docker Release Script         ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝${C_RESET}"

    log_info "Version: ${VERSION:-<auto>}"
    log_info "Skip Build: $SKIP_BUILD"
    log_info "Skip Push: $SKIP_PUSH"
    echo

    # Determine paths
    local hosts_dir="$SCRIPT_DIR"
    local src_dir
    src_dir="$(dirname "$hosts_dir")"
    local repo_root
    repo_root="$(dirname "$src_dir")"

    log_info "Repository Root: $repo_root"
    log_info "Source Directory: $src_dir"
    echo

    # Check if Docker is available
    if ! docker --version >/dev/null 2>&1; then
        log_error "Docker is not installed or not in PATH"
        exit 1
    fi

    # Load or configure registry
    local has_config=false
    if load_registry_config; then
        has_config=true
    fi

    if [ "$CONFIGURE_REGISTRY" = true ] || { [ "$has_config" = false ] && [ "$SKIP_PUSH" = false ]; }; then
        if initialize_registry_config; then
            has_config=true
        else
            has_config=false
        fi
    fi

    # Define image names
    local server_image_name="budget-server"
    local legacy_client_image_name="budget-client"   # old intermediate image, cleaned up only

    # Determine registry prefix for remote tags
    local registry_prefix=""
    if [ "$has_config" = true ]; then
        registry_prefix="$REGISTRY"
        if [ -n "$REGISTRY_NAMESPACE" ]; then
            registry_prefix="$registry_prefix/$REGISTRY_NAMESPACE"
        fi
    fi

    # Auto-generate version if not provided
    if [ -z "$(trim "$VERSION")" ]; then
        log_step "Auto-generating next version"
        VERSION="$(get_next_version "$repo_root" "$server_image_name")"
        log_success "Generated version: $VERSION"
    fi

    # Create git tag before building images
    ensure_git_tag "$repo_root" "$VERSION"

    # Local and remote tags
    local server_local_tag="${server_image_name}:${VERSION}"
    local server_remote_tag=""
    if [ "$has_config" = true ]; then
        server_remote_tag="${registry_prefix}/${server_image_name}:${VERSION}"
    fi

    # Build image
    if [ "$SKIP_BUILD" = false ]; then
        log_step "Starting Docker Image Build"

        local server_dockerfile="$hosts_dir/NVs.Budget.Hosts.Web.Server/Dockerfile"
        if ! build_docker_image "Server (with embedded client)" "$server_dockerfile" "$repo_root" "$server_local_tag"; then
            log_error "Server build failed. Aborting."
            exit 1
        fi

        echo
        log_success "Image built successfully!"

        # Display local images
        log_step "Local Images Built"
        docker images | grep -E "REPOSITORY|$server_image_name" || true
    else
        log_warn "Skipping build phase"
    fi

    # Push images to registry
    if [ "$SKIP_PUSH" = false ] && [ "$has_config" = true ]; then
        # Login to registry
        if ! connect_docker_registry; then
            log_warn "Failed to login to registry. Skipping push."
        else
            if publish_docker_image "$server_local_tag" "$server_remote_tag" "Server"; then
                echo
                log_success "Server image published successfully!"
                echo
                log_info "Server Image: $server_remote_tag"
                log_info "Note: Client assets are embedded in the server image"
            else
                echo
                log_error "Some images failed to publish"
                exit 1
            fi
        fi
    elif [ "$SKIP_PUSH" = false ]; then
        log_warn "No registry configured. Images are available locally only."
        log_info "Run with --configure-registry to set up Docker registry."
    else
        log_warn "Skipping push phase"
    fi

    # Cleanup old images if requested
    if [ "$CLEANUP_OLD_IMAGES" = true ]; then
        remove_old_images "$server_image_name" "$registry_prefix" "$KEEP_VERSIONS"
        remove_old_images "$legacy_client_image_name" "" "$KEEP_VERSIONS"
    fi

    echo
    log_step "Release Process Complete"
    echo
    log_success "Local image is tagged as:"
    printf '  - %s (client assets embedded in wwwroot)\n' "$server_local_tag"

    if [ "$has_config" = true ] && [ "$SKIP_PUSH" = false ]; then
        echo
        log_success "Remote images are available at:"
        printf '  - %s (includes embedded client)\n' "$server_remote_tag"
    fi

    echo
}

# Parse command line arguments
while [ $# -gt 0 ]; do
    case "$1" in
        --version)
            [ $# -ge 2 ] || { echo "Error: --version requires a value" >&2; exit 1; }
            VERSION="$2"; shift 2 ;;
        --skip-build) SKIP_BUILD=true; shift ;;
        --skip-push) SKIP_PUSH=true; shift ;;
        --configure-registry) CONFIGURE_REGISTRY=true; shift ;;
        --cleanup-old-images) CLEANUP_OLD_IMAGES=true; shift ;;
        --keep-versions)
            [ $# -ge 2 ] || { echo "Error: --keep-versions requires a value" >&2; exit 1; }
            KEEP_VERSIONS="$2"; shift 2 ;;
        -h|--help) usage; exit 0 ;;
        *)
            echo "Unknown option: $1" >&2
            usage
            exit 1 ;;
    esac
done

main
