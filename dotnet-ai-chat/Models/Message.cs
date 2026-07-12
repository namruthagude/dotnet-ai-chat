using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_ai_chat.Models
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ConversationId { get; set; }

        public Conversation? Conversation { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty ;

        public string? ModelUsed { get; set; }

        public int? InputTokens { get; set; }

        public int? OutputTokens { get; set; }

        public DateTime? Created { get; set; } = DateTime.UtcNow;
    }
}
