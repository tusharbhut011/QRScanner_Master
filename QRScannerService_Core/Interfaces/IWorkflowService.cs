using System.Collections.Generic;
using QRScannerService_Core.Models;


namespace QRScannerService_Core.Interfaces
{
    public interface IWorkflowService
    {
        void AddWorkflow(WorkflowConfig workflow);
        WorkflowConfig GetWorkflowForPrefix(string prefix);
        List<WorkflowConfig> GetAllWorkflows();
    }
}