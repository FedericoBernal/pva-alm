// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Translation.Helpers;
using Newtonsoft.Json;
using TranslationBot.Translation.Helpers;

namespace Microsoft.Translation
{
    /// <summary>
    /// Middleware for translating text between the user and bot.
    /// Uses the Microsoft Translator Text API.
    /// </summary>
    public class TranslationMiddleware : IMiddleware
    {
        private readonly MicrosoftTranslator _translator;
        private readonly UserState _userState;
        private readonly bool _detectLanguageOnce;
        private readonly bool _getLanguageFromUri;
        private readonly UserLanguage _languages;
        private IStatePropertyAccessor<string> _stateLanguage;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationMiddleware"/> class.
        /// </summary>
        /// <param name="translator">Translator implementation to be used for text translation.</param>
        public TranslationMiddleware(MicrosoftTranslator translator, IConfiguration configuration, UserState userState, UserLanguage languages)
        {
            _detectLanguageOnce = Convert.ToBoolean(configuration["DetectLanguageOnce"]);
            _getLanguageFromUri = Convert.ToBoolean(configuration["GetLanguageFromUri"]);
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
            _userState = userState ?? throw new NullReferenceException(nameof(userState));
            _stateLanguage = userState.CreateProperty<string>("UserLanguage");
            _languages = languages ?? throw new ArgumentNullException(nameof(languages));
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
            var language = string.Empty;
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var urlLanguage = _languages.Get(TranslationSettings.DefaultDictionaryKey);
                var userlanguage = await _stateLanguage.GetAsync(turnContext, () => string.Empty, cancellationToken);
                if ((_detectLanguageOnce && string.IsNullOrEmpty(userlanguage)) && !_getLanguageFromUri || !_detectLanguageOnce)
                {
                    language = await _translator.DetectAsync(turnContext.Activity.Text, cancellationToken);
                }
                else if ((_getLanguageFromUri && string.IsNullOrEmpty(userlanguage)) && !string.IsNullOrEmpty(urlLanguage))
                {
                    language = urlLanguage;
                }
                else
                {
                    language = userlanguage;
                }

                await _stateLanguage.SetAsync(turnContext, language, cancellationToken);
                await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
                turnContext.Activity.Text = await _translator.TranslateAsync(turnContext.Activity.Text, cancellationToken);
            }

            turnContext.OnSendActivities(async (newContext, activities, nextSend) =>
            {
                List<Task> tasks = new List<Task>();
                foreach (Activity currentActivity in activities.Where(a => a.Type == ActivityTypes.Message))
                {
                    var language = await _stateLanguage.GetAsync(newContext, () => TranslationSettings.DefaultLanguage, cancellationToken);
                    tasks.Add(TranslateMessageActivityAsync(currentActivity.AsMessageActivity(), language));
                }

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
                    var language = await _stateLanguage.GetAsync(newContext, () => TranslationSettings.DefaultLanguage, cancellationToken);
                    await TranslateMessageActivityAsync(activity.AsMessageActivity(), language);
                }

                return await nextUpdate();
            });

            await next(cancellationToken).ConfigureAwait(false);
        }

        private async Task TranslateMessageActivityAsync(IMessageActivity activity, string language, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity.Type == ActivityTypes.Message)
            {
                if (activity.Text != null)
                {
                    activity.Text = await _translator.TranslateAsync(activity.Text, cancellationToken, language);
                }

                if (activity.SuggestedActions != null)
                {
                    foreach (var action in activity.SuggestedActions.Actions)
                    {
                        action.Title = await _translator.TranslateAsync(action.Title, cancellationToken, language);
                        action.Value = await _translator.TranslateAsync(action.Value.ToString(), cancellationToken, language);
                    }
                }

                if (activity.Attachments != null)
                {
                    foreach (var attachment in activity.Attachments)
                    {
                        var stringContent = attachment.Content.ToString();
                        Regex regex = new Regex(@"(?<=\btext"": ""|title"": ""|value"": "")[^""]*");
                        var matches = regex.Matches(stringContent);
                        
                        foreach (var match in matches)
                        {
                            if (!string.IsNullOrEmpty(match.ToString()))
                            {
                                var translatedText = await _translator.TranslateAsync(match.ToString(), cancellationToken, language);
                                stringContent = stringContent.Replace(match.ToString(), translatedText);
                            }
                        }

                        attachment.Content = JsonConvert.DeserializeObject(stringContent);
                    }
                }

                // Bridge the bot message for Omnichannel support
                OmnichannelBotClient.BridgeBotMessage(activity);
            }
        }
    }
}


