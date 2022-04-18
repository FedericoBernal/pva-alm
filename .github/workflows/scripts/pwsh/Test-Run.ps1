function Export-Unpack-Commit ($githubRef, $workflow_name, $solution_name, $environment_url, $source_branch, $branch_to_create, $commit_message) {
    gh workflow run $workflow_name --ref $githubRef -f solution_name=$solution_name -f environment_url=$environment_url -f source_branch=$source_branch -f branch_to_create=$branch_to_create -f commit_message=$commit_message

    Wait-Workflow-Execution $workflow_name
}
function Sync-Unmanaged ($githubRef, $workflow_name, $from_branch, $to_branch) {
    gh workflow run $workflow_name --ref $githubRef -f from_branch=$from_branch -f to_branch=$to_branch

    Wait-Workflow-Execution $workflow_name
}

function Delete-Import-Unmanaged ($workflow_name, $solution_name, $environment_url, $githubRef) {
    gh workflow run $workflow_name --ref $githubRef -f solution_name=$solution_name -f environment_url=$environment_url -f ref=$githubRef

    Wait-Workflow-Execution $workflow_name
}

function Wait-Workflow-Execution ($workflow_name) {
    Do
    {
        $cmdOutput = gh workflow view $workflow_name | Out-String
        $firstLine = ($cmdOutput -split '\n')[5]
        echo "workflow line: $firstLine"
        $colArray = $firstLine -split '	'
        $status = $colArray[0]
        $success = $colArray[1]
        echo "$workflow_name status: $status - sucess: $success" 
        Start-Sleep -Seconds 15
    } While ($status -ne "completed")
}