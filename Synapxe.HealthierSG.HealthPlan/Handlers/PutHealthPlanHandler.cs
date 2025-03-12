using Hl7.Fhir.Model;
using Ihis.FhirEngine.Core;
using Task = System.Threading.Tasks.Task;

namespace Synapxe.HealthierSG.HealthPlan.Handlers;

[FhirHandlerClass(AcceptedType = nameof(CarePlan))]
public class PutHealthPlanHandler
{
    [FhirHandler(HandlerCategory.PreInteraction, FhirInteractionType.Create, sort: 99)]
    public Task PreInteractionPrepareDataForCreatingHealPlanAsync(IFhirContext context, CancellationToken cancellationToken)
    {
        var healthPlan = (context.Request.Resource as CarePlan)!;

        AssignCarePlanSubjectToTargetSubjects(healthPlan);

        SetGoalAndConditionProfileMetaData(healthPlan);

        return Task.CompletedTask;
    }

    private static void SetGoalAndConditionProfileMetaData(CarePlan healthPlan)
    {
        if (ContainsDuplicateGoals(healthPlan))
            throw new Exception("Duplicate goals found");

        foreach (Resource resource in healthPlan.Contained)
        {
            switch (resource)
            {
                case Goal goal:
                    goal.Meta = new Meta { Profile = [$"https://synapxe.sg/profile/{goal.Description.Coding[0].Code}"] };
                    break;
                case Condition condition:
                    condition.Meta = new Meta { Profile = [$"https://synapxe.sg/profile/C1"] };
                    break;
            }
        }
    }

    private static bool ContainsDuplicateGoals(CarePlan carePlan)
    {
        var containedArray = carePlan.Contained;
        var goalResources = containedArray.Where(resource => resource is Goal).Cast<Goal>().ToList();
        bool duplicateGoals = goalResources
        .GroupBy(goal => goal.Description.Coding[0].Code)
        .Any(group => group.Count() > 1);

        return duplicateGoals;
    }

    private static void AssignCarePlanSubjectToTargetSubjects(CarePlan healthPlan)
    {
        ResourceReference subject = healthPlan.Subject;
        foreach (Resource resource in healthPlan.Contained)
        {
            switch (resource)
            {
                case Goal goal:
                    goal.Subject = subject;
                    break;
                case Condition condition:
                    condition.Subject = subject;
                    break;
            }
        }
    }

}
