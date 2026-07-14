using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dotnet_ai_chat
{
    public class HeaderCapturePolicy : PipelinePolicy
    {
        public static string? RemainingTokens;
        public static string? LimitTokens;
        public static string? RemainingRequests;
        public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
        {
            ProcessNext(message, pipeline, currentIndex);
            Capture(message);
        }

        public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
        {
            await ProcessNextAsync(message, pipeline, currentIndex);
            Capture(message);
        }

        private static void Capture(PipelineMessage message)
        {
            var headers = message.Response?.Headers;
            if (headers == null) return;

            headers.TryGetValue("x-ratelimit-remaining-tokens", out RemainingTokens);
            headers.TryGetValue("x-ratelimit-limit-tokens", out LimitTokens);
            headers.TryGetValue("x-ratelimit-remaining-requests", out RemainingRequests);
        }
    }
}
