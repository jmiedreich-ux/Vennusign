using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.PlatformOperations;
using Vennu.Api.TestAgent;

namespace Vennu.Api.Controllers.PlatformOperations;

[ApiController]
[Route("api/platform-operations/test-agent/runs")]
[Authorize(Policy = PlatformOperationsAuthenticationDefaults.AuthorizationPolicy)]
public sealed class PlatformOperationsTestAgentController(ITestAgentRunService runs) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<TestAgentRun>> List() => Ok(runs.List());

    [HttpGet("{id:guid}")]
    public ActionResult<TestAgentRun> Get(Guid id) => runs.Get(id) is { } run ? Ok(run) : NotFound();

    [HttpPost]
    public ActionResult<TestAgentRun> Start(StartTestAgentRunRequest request)
    {
        try { var run = runs.Start(request); return AcceptedAtAction(nameof(Get), new { id = run.Id }, run); }
        catch (ArgumentException exception) { return ValidationProblem(new Dictionary<string, string[]> { [nameof(request.Mission)] = [exception.Message] }); }
        catch (InvalidOperationException exception) { return Problem(exception.Message, statusCode: StatusCodes.Status409Conflict); }
    }

    [HttpPost("{id:guid}/cancel")]
    public IActionResult Cancel(Guid id) => runs.Cancel(id) ? Accepted() : NotFound();
}
