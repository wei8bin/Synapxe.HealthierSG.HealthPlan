@Environment:Integration
Feature: CarePlan

Background:
    Given a random tag
    And a new HttpClient as httpClient
        | HeaderName | Value   |
        | x-api-key  | testapp |

Scenario: Reading a newly created healthplan returns exactly the same healthplan
    Given a Resource is created from Samples/healthplan_goal_1.json as createdHealthPlan
    Then createdHealthPlan is a Fhir CarePlan with data
        | Path       | Value |
        | statusCode | 201   |
