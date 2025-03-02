export interface AuthResponse {
    isAuthenticated: boolean;
    user?: {
      id: string
    };
    owner?: {
      id: string;
      name: string;
    }
  }