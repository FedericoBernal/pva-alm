const dispatchWorkflow = async (github, context, id, reference, parameters) => {
  await github.rest.actions.createWorkflowDispatch({
    owner: context.repo.owner,
    repo: context.repo.repo,
    workflow_id: id,
    ref: reference,
    inputs: parameters
  })
}

const checkWorkflowStatus = async (github, context, core, id) => {
  let currentStatus = null;
  let conclusion = null;
  let html_url = null;
  function wait(ms){
    var start = new Date().getTime();
    var end = start;
    while(end < start + ms) {
      end = new Date().getTime();
   }
 }
  
  console.log('Checking the status for workflow ' + id)
  function wait(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
  do {
    let workflowLog = await github.rest.actions.listWorkflowRuns({
      owner: context.repo.owner,
      repo: context.repo.repo,
      workflow_id: id,
      per_page: 1
    })
    wait(20000)
    if (workflowLog.data.total_count > 0) {
      currentStatus = workflowLog.data.workflow_runs[0].status
      conclusion = workflowLog.data.workflow_runs[0].conclusion
      html_url = workflowLog.data.workflow_runs[0].html_url
    }
    else {
      break
    }
    console.log(new Date().toISOString() + ' - status: ' + currentStatus)
  } while (currentStatus != 'completed');

  if (conclusion != 'success') {
    core.setFailed('Workflow execution failed. For more details go to ' + html_url)
  }
}

module.exports = {
  dispatchWorkflow,
  checkWorkflowStatus
}