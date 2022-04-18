function Run-Workflow ($githubRef, $environment, $tag) {
    echo "init Run-Workflow"
    gh workflow run deploy-tagged-solution-to-environment --ref $githubRef -f $environment -f $tag
    echo "sleep for 5 seconds"
    Start-Sleep -Seconds 5
    $cmdOutput = gh workflow view deploy-tagged-solution-to-environment | Out-String
    echo "cmdOutput : $cmdOutput" 
    echo "end Run-Workflow"
}
