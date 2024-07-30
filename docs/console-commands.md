* budget
    * acc - accounts
        * stats - accounts-based statistics
        * merge - merges two accounts into one by moving operation from one account to another
    * ops (o) - operations
        * list - list operations
        * list-duplicates - get list of duplicated operations
        * import - perform import
        * update - update operations from file
        * retag - update the list of tags
        * remove - remove operations
    * owners - owners
        * list - list owners
        * self-register - register current user as owner
    * xfers (x) - transfers
        * register - manually register transfers
        * remove - removes registered transfers by source ids
    * admin - administrative actions
        * settings - list effective settings
        * migrate-db - apply db migrations
    * test - tests for configuration
        * import - test csv reading options for particular file
