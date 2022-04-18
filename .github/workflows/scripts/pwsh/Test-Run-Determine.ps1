function Run-Workflow ($githubRef, $solution_name) {
    echo "init Run-Workflow"
    gh workflow run determine-solution-build-deploy --ref $githubRef -f solution_name=$solution_name
    echo "sleep for 5 seconds"
    Start-Sleep -Seconds 5
    $cmdOutput = gh workflow view determine-solution-build-deploy | Out-String
    echo "cmdOutput : $cmdOutput" 
    echo "end Run-Workflow"
}
