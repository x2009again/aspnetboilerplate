﻿using Abp.Dependency;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Abp.HtmlSanitizer.ActionFilter;

public interface IActionFilterHtmlSanitizerHelper : ISingletonDependency
{
    bool ShouldSanitizeContext(ActionExecutingContext actionExecutingContext);
        
    void SanitizeContext(ActionExecutingContext context);
}