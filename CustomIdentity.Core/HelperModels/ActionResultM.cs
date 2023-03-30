namespace CustomIdentity.Core.HelperModels
{
	public class ActionResultM<T> where T : class
	{
		public T Data { get; set; }
		public ActionStatus Status { get; private set; }
		public string Message { get; set; }


		public ActionResultM(T _data)
		{
			Data = _data;
			Status = ActionStatus.Success;
		}

		public ActionResultM(string _status, string _message)
		{
			if (_status == "OK")
			{
				Message = _message;
				Status = ActionStatus.Success;
			}
			else
			{
				Message = _message;
				Status = ActionStatus.Error;
			}
		}

		public ActionResultM(string _message)
		{
			Message = _message;
			Status = ActionStatus.Error;
		}

		public ActionResultM(Exception ex)
		{
			Message = ex.Message;
			Status = ActionStatus.FatalError;
		}

	}

	public enum ActionStatus
	{
		Success,
		Error,
		FatalError,
	}
}
