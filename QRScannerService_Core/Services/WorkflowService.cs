using System.Collections.Generic;
using System.Linq;

using QRScannerService_Core.Interfaces;
using QRScannerService_Core.Models;

namespace QRScannerService_Core.Services
{
    public class WorkflowService : IWorkflowService
    {
        private List<WorkflowConfig> _workflows;

        public WorkflowService()
        {
            _workflows = new List<WorkflowConfig>();
        }

        public void AddWorkflow(WorkflowConfig workflow)
        {
            _workflows.Add(workflow);
        }

        public WorkflowConfig GetWorkflowForPrefix(string prefix)
        {
            return _workflows.FirstOrDefault(w => w.Prefix == prefix);
        }

        public List<WorkflowConfig> GetAllWorkflows()
        {
            return _workflows;
        }
    }
}
