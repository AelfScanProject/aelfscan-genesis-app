using AeFinder.App.TestBase;
using AeFinder.Sdk.Processor;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Types;

namespace AElfScan.GenesisApp;

public abstract class GenesisAppTestBase: AeFinderAppTestBase<GenesisAppTestModule>
{
    
    protected Address TestAddress = Address.FromBase58("ooCSxQ7zPw1d4rhQPBqGKB6myvuWbicCiw3jdcoWEMMpa54ea");
    protected string ChainId = "AELF";
    protected string BlockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
    protected string PreviousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
    protected string TransactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    protected string FromAddress = "BNPFPPwQ3DE9rwxzdY61Q2utU9FZx9KYUnrYHQqCR6N4LLhUE";
    protected long BlockHeight = 100;
    
    
    protected LogEventContext GenerateLogEventContext<T>(T eventData) where T : IEvent<T>
    {
        var logEvent = eventData.ToLogEvent().ToSdkLogEvent();
        
        return new LogEventContext
        {
            ChainId = ChainId,
            Block = new LightBlock
            {
                BlockHash = BlockHash,
                BlockHeight = BlockHeight,
                BlockTime = DateTime.UtcNow,
                PreviousBlockHash = PreviousBlockHash
            },
            Transaction = new AeFinder.Sdk.Processor.Transaction()
            {
                TransactionId = TransactionId,
                From = FromAddress
            },
            LogEvent = logEvent
        };
    }
    
}