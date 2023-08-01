using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace lightc
{
    public class MyEvent
    {
        public string Event { get; set; }=string.Empty;
    }

    public class FunctionTest
    {
        public string FunctionHandler(MyEvent myEvent, ILambdaContext context)
        {
            return myEvent.Event.ToUpper();
        }
    }
}