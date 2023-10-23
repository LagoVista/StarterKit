﻿using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.StarterKit.Models;
using LagoVista.ProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.IoT.StarterKit
{
    public interface IAppWizard
    {
        Task<InvokeResult<Project>> CreateProjectAsync(AppWizardRequest request, EntityHeader org, EntityHeader user);
    }
}
