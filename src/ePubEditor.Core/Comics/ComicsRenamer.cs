using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace ePubEditor.Core.Comics
{
    /// <summary>
    /// Renames comics using OpenAI service for intelligent suggestions.
    /// </summary>
    /// <param name="chatClient">Injected OpenAI chat client.</param>
    /// <example>
    /// <code>
    /// IChatClient chatClient = ...;
    /// ComicsRenamer renamer = new ComicsRenamer(chatClient);
    /// </code>
    /// </example>
    public class ComicsRenamer
    {
        private readonly IChatClient _chatClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComicsRenamer"/> class.
        /// </summary>
        /// <param name="chatClient">The OpenAI chat client to use for renaming operations.</param>
        public ComicsRenamer(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }
    }
}
