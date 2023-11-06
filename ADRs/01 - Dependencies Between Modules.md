# 01 - Dependencies between modules

## Reported by: wiktormuller
## Approved by: wiktormuller

## Context
    In the context of selected architecture which is Modular Monolith, we should some particular way of integration which:
        - allow us to communicate between modules
        - will ensure encapsulation of implementation details in modules.
    
    Without any strict rules, there is possibility of referencing between modules bypassing their public API.

    This may lead to creating logic in module A based on implementation details in module B (broken encapsulation), that should be able to change those details independently of their consumers.

## Concerns
    Communication options:
        - Shared Contracts - projects named <Module>.Shared with interface representing it's public API.
        - Local Contracts - modules communicate with each other via addresses that are similar to HTTP (with serialization for transport purposes and types translation).

    References between modules that bypass public APIs:
        - Verbal prohibition on the creation of such references.
        - Creating architecture tests, which will analyze whether such phenomena don't occur.
        - Decomposition of the solution into smaller solutions (per module) and merging them into one deployment artefact during build process.

## Decision
    Communication options -> we decided for local contracts, because we can exclude completely implicit references between modules.

    References options -> we decided for option with architecture tests to be sure that those rules won't be broken.

## Expecting result
    Established way of creating dependencies between modules allow modules to communicate with each other and at the same time maintain their encapsulation.

    If developers will fail to comply with the established rules the architecture tests will protect them.
