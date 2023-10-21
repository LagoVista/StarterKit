﻿using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.StarterKit.Services;
using LagoVista.ProjectManagement;
using LagoVista.ProjectManagement.Models;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit.Tests
{
    [TestClass]
    public class YamlBuilderTests
    {
        IYamlServices _yamlSerivces;
        Mock<ISurveyManager> _surveyManager = new Mock<ISurveyManager>();

        IStarterKitConnection _starterKitConnection = new StarterKitConnection();
        Mock<IOrganizationRepo> _orgRepo = new Mock<IOrganizationRepo>();

        const string ORIG_SURVEY_ID = "12345678";
        const string CHILD_SURVEY_ID = "91011121314";
        const string CHILD_SURVEY_KEY = "childsurveykey";
        EntityHeader _org = new EntityHeader();
        EntityHeader _user = new EntityHeader();

        [TestInitialize]
        public void Init()
        {
            _starterKitConnection.StarterKitStorage.Uri = "http://dontcare";
            _starterKitConnection.StarterKitStorage.AccessKey = "http://dontcare";
            _starterKitConnection.StarterKitStorage.ResourceName = "http://dontcare";

            _orgRepo.Setup(orr => orr.GetOrganizationAsync(It.IsAny<string>())).ReturnsAsync(new UserAdmin.Models.Orgs.Organization()
            {
                Namespace = "ORGNS"
            });

            _yamlSerivces = new YamlServices(new Mock<IAdminLogger>().Object, _starterKitConnection, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                 _orgRepo.Object, null, null, _surveyManager.Object, null, null, null, null);
        }

        [TestMethod]
        public async Task GetYaml()
        {
            _surveyManager.Setup(mgr => mgr.GetSurveyAsync(CHILD_SURVEY_ID, It.IsAny<EntityHeader>(), It.IsAny<EntityHeader>())).ReturnsAsync(() =>
            {
                return new Survey()
                {
                    Name = "Child Survey 1",
                    Key = CHILD_SURVEY_KEY
                };
            });

            _surveyManager.Setup(mgr => mgr.GetSurveyByKeyAsync(CHILD_SURVEY_KEY, It.IsAny<EntityHeader>(), It.IsAny<EntityHeader>())).ReturnsAsync(() =>
            {
                return new Survey()
                {
                    Id = Guid.NewGuid().ToId(),
                    Name = "Child Survey 1",
                    Key = CHILD_SURVEY_KEY
                };
            });

            _surveyManager.Setup(mgr => mgr.GetSurveyAsync(ORIG_SURVEY_ID, It.IsAny<EntityHeader>(), It.IsAny<EntityHeader>())).ReturnsAsync(() =>
            {
                return new Survey()
                {
                    Name = "Survey",
                    Key = "surveykey",
                    Id = ORIG_SURVEY_ID,
                    SurveyType = EntityHeader<SurveyTypes>.Create(SurveyTypes.IoTApp),
                    QuestionSets = new List<SurveyQuestionSet>()
                    {
                         new SurveyQuestionSet()
                         {
                             Id = Guid.NewGuid().ToId(),
                             Title = "Question Set 2",
                             Name = "QS2",
                             Key = "qs2",
                             Description = "question set 2",
                             Questions = new List<SurveyQuestion>()
                             {
                                 new SurveyQuestion()
                                 {
                                     Id = Guid.NewGuid().ToId(),
                                      QuestionType = EntityHeader<QuestionTypes>.Create(QuestionTypes.SingleLineText),
                                      HelpText = "This is a survry2",
                                      QuestionText = "What is this question3"
                                 },
                                 new SurveyQuestion()
                                 {
                                     Id = Guid.NewGuid().ToId(),
                                      QuestionType = EntityHeader<QuestionTypes>.Create(QuestionTypes.ChildSurveys),
                                      HelpText = "This is a survry2",
                                      QuestionText = "What is this question3",
                                      ChildSurveyType = EntityHeader.Create(CHILD_SURVEY_ID, CHILD_SURVEY_KEY, "child survey")

                                 }
                             }
                         },
                         new SurveyQuestionSet()
                         {
                             Id = Guid.NewGuid().ToId(),
                             Title = "Question Set 2",
                             Name = "QS2",
                             Key = "qs2",
                             Description = "question set 2",
                             Questions = new List<SurveyQuestion>()
                             {
                                 new SurveyQuestion()
                                 {
                                     Id = Guid.NewGuid().ToId(),
                                      QuestionType = EntityHeader<QuestionTypes>.Create(QuestionTypes.SingleLineText),
                                      HelpText = "This is a survry2",
                                      QuestionText = "What is this question3"
                                 },
                                 new SurveyQuestion()
                                 {
                                     Id = Guid.NewGuid().ToId(),
                                      QuestionType = EntityHeader<QuestionTypes>.Create(QuestionTypes.SingleLineText),
                                      HelpText = "This is a survry",
                                      QuestionText = "What is this question"
                                 }

                             }
                         },
                    }

                };
            });

            var result = await _yamlSerivces.SerilizeToYamlAsync(nameof(LagoVista.ProjectManagement.Models.Survey), ORIG_SURVEY_ID, _org, _user);
            Console.WriteLine(result.Result.Item1);

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(result.Result.Item1);
            writer.Flush();
            stream.Position = 0;

            var copiedSurvey = await _yamlSerivces.DeserializeFromYamlAsync(nameof(LagoVista.ProjectManagement.Models.Survey), stream, _org, _user);
        }

    }

    public class StarterKitConnection : IStarterKitConnection
    {
        IConnectionSettings _settings = new ConnectionSettings();

        public IConnectionSettings StarterKitStorage => _settings;
    }
}
