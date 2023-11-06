# 01 - Dependencies between modules

## Reported by: wiktormuller
## Approved by: wiktormuller

## Context
    Almost every IT system operates on time and dates.
    
    In order to avoid inconsistencies (or unnecessary conversions), the way dates are stored and transmitted throughout the system should be standardized.

## Concerns
   - Using local time, like Polish.
   - Using UTC.

## Decision
    The simplest decision (and at the same time the less error-prone) is using UTC time.

    Possible issues of localization should be delegated to client applications, for example front-end app or mobile app.

## Expecting result
    Assuming, that every date in our system is in UTC format will minimize problems (and tasks) associated with storing it in the database or in the transport.

    At the same time, app's clients have clear API contact that can depends on when presenting or transferring dates.
