module.exports = async ({ github, context }) => {

    async function runWorkflow() {
        console.log('init runWorkflow')
        await github.rest.actions.createWorkflowDispatch({
            owner: context.repo.owner,
            repo: context.repo.repo,
            workflow_id: 'determine-solution-build-deploy.yml',
            ref: 'refs/heads/main'       
          })
    }
}
