namespace Client.Logic.Response
{
    public class ResumeResponse : BaseResponse
    {
        public override void Process()
        {
            if (Global.BlockingResponse == null)
            {
                base.Process();
                return;
            }

            var nextResponse = Global.BlockingResponse.NextResponse;
            
            Tail.NextResponse = nextResponse;
            NextResponse.Process();
        }
    }
}