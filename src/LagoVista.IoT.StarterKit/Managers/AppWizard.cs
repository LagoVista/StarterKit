// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0ab391756594a2f66c843384cd2157d01c668c3909744f12e647ae25a6f19912
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Deployment.Admin;
using LagoVista.IoT.DeviceAdmin.Interfaces.Managers;
using LagoVista.IoT.Pipeline.Admin.Managers;
using LagoVista.IoT.StarterKit.Models;
using LagoVista.ProjectManagement;
using LagoVista.ProjectManagement.Core;
using LagoVista.ProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Managers
{
    public class AppWizard : IAppWizard
    {
        ISurveyResponseManager _surveyResponseManager;
        IProjectManager _projectManager;
        IProjectTemplateManager _projectTemplateManger;
        IPipelineModuleManager _pipelineMgr;
        IDeviceConfigurationManager _deviceCfgMgr;
        IDeviceTypeManager _deviceTypeMgr;
        ITaskManager _taskManger;

        public AppWizard(ISurveyResponseManager responseManager, ITaskManager taskManger, IProjectManager projectManager, IPipelineModuleManager pipelineMgr, IDeviceTypeManager deviceTypeMgr,
                         IDeviceConfigurationManager deviceCfgMgr, IProjectTemplateManager projectTemplateManager)
        {
            _surveyResponseManager = responseManager ?? throw new ArgumentNullException(nameof(responseManager));
            _projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
            _pipelineMgr = pipelineMgr ?? throw new ArgumentNullException(nameof(pipelineMgr));
            _deviceTypeMgr = deviceTypeMgr ?? throw new ArgumentNullException(nameof(deviceTypeMgr));
            _deviceCfgMgr = deviceCfgMgr ?? throw new ArgumentNullException(nameof(deviceCfgMgr));
            _projectTemplateManger = projectTemplateManager ?? throw new ArgumentNullException(nameof(projectTemplateManager));
            _taskManger = taskManger ?? throw new ArgumentNullException(nameof(taskManger));
        }

        public async Task<InvokeResult<Project>> CreateProjectAsync(AppWizardRequest appWizardRequest, EntityHeader org, EntityHeader user)
        {
            if (!String.IsNullOrEmpty(appWizardRequest.SurveyResponseId))
                return await CreateProjectForSurveyAsync(appWizardRequest, org, user);
            else if (!EntityHeader.IsNullOrEmpty(appWizardRequest.ProjectTemplate))
                return await CreateProjectForProjectTemplateAsync(appWizardRequest, org, user);

            throw new NotImplementedException("App/Project Wizard only support survey response and project template.");
        }

        private async Task<InvokeResult<Project>> CreateProjectForProjectTemplateAsync(AppWizardRequest appWizardRequest, EntityHeader org, EntityHeader user)
        {
            var timeStamp = DateTime.UtcNow.ToJSONString();

            var project = new Project()
            {
                CreatedBy = user,
                LastUpdatedBy = user,
                OwnerOrganization = org,
                CreationDate = timeStamp,
                LastUpdatedDate = timeStamp,
                Description = appWizardRequest.Description,
                Name = appWizardRequest.ProjectName,
                Key = appWizardRequest.ProjectCode.ToLower(),
                ProjectAdminLead = appWizardRequest.ProjectAdminLead,
                ProjectLead = appWizardRequest.ProjectLead,
                DefaultQAResource = appWizardRequest.DefaultQAResource,
                DefaultPrimaryContributor = appWizardRequest.DefaultPrimaryContributor,
                ProjectCode = appWizardRequest.ProjectCode,
                Status = EntityHeader<ProjectStatus>.Create(ProjectStatus.InProcess),
            };

        
            var tasks = new List<WorkTask>();

            var result = await _projectManager.AddProjectAsync(project, org, user);
            if (!result.Successful)
                return InvokeResult<Project>.FromInvokeResult(result);

            var projectEH = EntityHeader.Create(project.Id, project.Key, project.Name);
            var template = await _projectTemplateManger.GetProjectTemplateAsync(appWizardRequest.ProjectTemplate.Id, org, user);
        
            foreach(var taskTemplate in template.TaskTemplates)
            {
                Console.WriteLine($"[AppWizard__CreateProjectForProjectTemplateAsync] - Task Template: {taskTemplate.Text}");
                var createdTask = await _taskManger.CreateAndSaveTaskFromTemplateAsync(taskTemplate.Id, projectEH, org, user);
            }

            var newProject = await _projectManager.GetProjectAsync(project.Id, org, user);
            return InvokeResult<Project>.Create(newProject);
        }

        private async Task<InvokeResult<Project>> CreateProjectForSurveyAsync(AppWizardRequest appWizardRequest, EntityHeader org, EntityHeader user)
        {

            var timeStamp = DateTime.UtcNow.ToJSONString();

            var project = new Project()
            {
                CreatedBy = user,
                LastUpdatedBy = user,
                OwnerOrganization = org,
                CreationDate = timeStamp,
                LastUpdatedDate = timeStamp,
                Description = appWizardRequest.Description,
                Name = appWizardRequest.ProjectName,
                ProjectAdminLead = appWizardRequest.ProjectAdminLead,
                ProjectLead = appWizardRequest.ProjectLead,
                DefaultQAResource = appWizardRequest.DefaultQAResource,
                DefaultPrimaryContributor = appWizardRequest.DefaultPrimaryContributor,
                Key = appWizardRequest.ProjectCode.ToLower(),
                ProjectCode = appWizardRequest.ProjectCode,
                Status = EntityHeader<ProjectStatus>.Create(ProjectStatus.InProcess),
            };

            var projectEH = EntityHeader.Create(project.Id, project.Key, project.Name);

            var response = await _surveyResponseManager.GetSurveyResponseAsync(appWizardRequest.SurveyResponseId, org.Id, org, user);

            project.Survey = EntityHeader.Create(response.Survey.Id, response.Survey.Key, response.Survey.Name);
            project.TopLevelSurveyResponse = EntityHeader.Create(response.Response.TopLevelSurveyResponseId, response.Response.SurveyResponseName);
            project.ParentSurveyResponse = EntityHeader.Create(response.Response.CurrentSurveyResponseId, response.Response.SurveyResponseName);
            await _projectManager.AddProjectAsync(project, org, user);

            foreach (var qs in response.Survey.QuestionSets)
            {
                var questions = new List<TaskSurveyQuestionSummary>();

                foreach (var question in qs.Questions)
                {
                    questions.Add(new TaskSurveyQuestionSummary()
                    {
                        QuestionId = question.Id,
                        Question = question.QuestionText,
                        QuestionKey = question.Key,
                    });

                    var answers = response.Answers.Where(ans => ans.QuestionId == question.Id);
                    foreach (var answer in answers)
                    {
                        var answerOption = question.Answers.FirstOrDefault(ao => ao.Id == answer.AnswerId);
                        if (answerOption != null && !EntityHeader.IsNullOrEmpty(answerOption.TaskTemplate))
                        {
                            var taskResult = await _taskManger.CreateAndSaveTaskFromTemplateAsync(answerOption.TaskTemplate.Id, projectEH, org, user);
                            var questionSummary = new TaskSurveyQuestionSummary()
                            {
                                TimeStamp = timeStamp,
                                Question = question.QuestionText,
                                QuestionKey = question.Key,
                                QuestionId = question.Id
                            };

                            questionSummary.Answers.Add(new TaskSurveyAnswerSummary()
                            {
                                TimeStamp = timeStamp,
                                Answer = answer.Answer,
                                AnswerKey = answerOption.Key
                            });

                            taskResult.Result.TaskSurveySummary = new TaskSurveySummary()
                            {
                                TimeStamp = timeStamp,
                                SurveyName = response.Survey.Name,
                                SurveyId = response.Survey.Id,
                                QuestionSet = qs.Title,
                                QuestionSetId = qs.Id,
                                QuestionSetKey = qs.Key
                            };
                            taskResult.Result.TaskSurveySummary.Questions.Add(questionSummary);

                            await _taskManger.AddWorkTaskAsync(taskResult.Result, org, user);
                        }
                    }

                    if (!EntityHeader.IsNullOrEmpty(question.TaskTemplate))
                    {
                        var taskResult = await _taskManger.CreateAndSaveTaskFromTemplateAsync(question.TaskTemplate.Id, projectEH, org, user);
                        var questionSummary = new TaskSurveyQuestionSummary()
                        {
                            TimeStamp = timeStamp,
                            Question = question.QuestionText,
                            QuestionKey = question.Key,
                            QuestionId = question.Id
                        };

                        foreach (var answer in answers)
                        {
                            var answerOption = question.Answers.FirstOrDefault(ao => ao.Id == answer.AnswerId);
                            if (answerOption != null)
                            {
                                questionSummary.Answers.Add(new TaskSurveyAnswerSummary()
                                {
                                    TimeStamp = timeStamp,
                                    Answer = answer.Answer,
                                    AnswerKey = answerOption.Key
                                });
                            }
                            else
                            {
                                questionSummary.Answers.Add(new TaskSurveyAnswerSummary()
                                {
                                    TimeStamp = timeStamp,
                                    Answer = answer.Answer,
                                });
                            }
                        }

                        taskResult.Result.TaskSurveySummary = new TaskSurveySummary()
                        {
                            TimeStamp = timeStamp,
                            SurveyName = response.Survey.Name,
                            SurveyId = response.Survey.Id,
                            QuestionSet = qs.Title,
                            QuestionSetId = qs.Id,
                            QuestionSetKey = qs.Key,
                        };

                        taskResult.Result.TaskSurveySummary.Questions.Add(questionSummary);
                        await _taskManger.AddWorkTaskAsync(taskResult.Result, org, user);
                    }
                }

                if (!EntityHeader.IsNullOrEmpty(qs.TaskTemplate))
                {
                    var taskResult = await _taskManger.CreateAndSaveTaskFromTemplateAsync(qs.TaskTemplate.Id, projectEH, org, user);
                    foreach (var question in qs.Questions)
                    {
                        var questionSummary = new TaskSurveyQuestionSummary()
                        {
                            TimeStamp = timeStamp,
                            Question = question.QuestionText,
                            QuestionKey = question.Key,
                            QuestionId = question.Id
                        };

                        var answers = response.Answers.Where(ans => ans.QuestionId == question.Id);
                        foreach (var answer in answers)
                        {
                            var answerOption = question.Answers.FirstOrDefault(ao => ao.Id == answer.AnswerId);
                            if (answerOption != null)
                            {
                                questionSummary.Answers.Add(new TaskSurveyAnswerSummary()
                                {
                                    TimeStamp = timeStamp,
                                    Answer = answer.Answer,
                                    AnswerKey = answerOption.Key
                                });
                            }
                            else
                            {
                                questionSummary.Answers.Add(new TaskSurveyAnswerSummary()
                                {
                                    TimeStamp = timeStamp,
                                    Answer = answer.Answer,
                                });
                            }
                        }

                        taskResult.Result.TaskSurveySummary.Questions.Add(questionSummary);
                    }
                    await _taskManger.AddWorkTaskAsync(taskResult.Result, org, user);
                }
            }

            return InvokeResult<Project>.Create(project);
        }
    }
}
