namespace GraphQL.Errors
{
    public class MainErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            var err = ErrorBuilder.New()
                .SetCode(error.Code)
                .SetPath(error.Path)
                .SetMessage(error.Message)
                .SetExtension("TimeStamp",DateTime.Now)
                .SetExtension("ErrorType", error?.Extensions?.GetValueOrDefault("ErrorType") ?? "MainFilter - set on stact trace and locataion to get more info")
                .Build();

          
            

            //if(error.Exception is not null)
            //{
            //    err.Exception!.GetType().ToString();
            //}

            return err;
                
        }
    }
}
