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
  sleep(2000)

  console.log('Checking the status for workflow ' + id)
  do {
    let workflowLog = await github.rest.actions.listWorkflowRuns({
      owner: context.repo.owner,
      repo: context.repo.repo,
      workflow_id: id,
      per_page: 1
    })
    if (workflowLog.data.total_count > 0) {
      currentStatus = workflowLog.data.workflow_runs[0].status
      conclusion = workflowLog.data.workflow_runs[0].conclusion
      html_url = workflowLog.data.workflow_runs[0].html_url
    }
    else {
      break
    }
    console.log(new Date().toISOString() + ' - status: ' + currentStatus)
    sleep(20000)
  } while (currentStatus != 'completed');

  if (conclusion != 'success') {
    core.setFailed('Workflow execution failed. For more details go to ' + html_url)
  }
}

const sleep = (milliseconds) => {
  const date = Date.now();
  let currentDate = null;
  do {
    currentDate = Date.now();
  } while (currentDate - date < milliseconds);
}

module.exports = {
  dispatchWorkflow,
  checkWorkflowStatus
}