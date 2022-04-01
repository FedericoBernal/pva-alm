// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Translation
{
    /// <summary>
    /// Middleware for translating text between the user and bot.
    /// Uses the Microsoft Translator Text API.
    /// </summary>
    public class Middleware : IMiddleware
    {
        private readonly UserState _userState;
        private readonly bool _detectLanguageOnce;
        private readonly string _defaultLang = "en";
        private IStatePropertyAccessor<string> _userLanguage;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationMiddleware"/> class.
        /// </summary>
        /// <param name="translator">Translator implementation to be used for text translation.</param>
        public Middleware( IConfiguration configuration, UserState userState)
        {
            _detectLanguageOnce = Convert.ToBoolean(configuration["DetectLanguageOnce"]);
            _userState = userState ?? throw new NullReferenceException(nameof(userState));
            _userLanguage = userState.CreateProperty<string>("UserLanguage");
        }

        /// <summary>
        /// Processes an incoming activity.
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
               

                
            }

            turnContext.OnSendActivities(async (newContext, activities, nextSend) =>
            {
                List<Task> tasks = new List<Task>();
              
                if (tasks.Any())
                {
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }

                return await nextSend();
            });

            turnContext.OnUpdateActivity(async (newContext, activity, nextUpdate) =>
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    var language = await _userLanguage.GetAsync(newContext, () => _defaultLang, cancellationToken);
                }

                return await nextUpdate();
            });

            await next(cancellationToken).ConfigureAwait(false);
        }

      
    }
}
