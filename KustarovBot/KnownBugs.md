1. Означает, что выскочила капча при попытке авторизациию Решение - получить вечный токен самому и заходить по нему.
Unhandled exception. VkNet.Exception.VkAuthorizationException: Failed to determine page type from url: https://m.vk.com:443/login?act=authcheck_code&hash=1609594843_9cfb90fa50956ff20b
   at VkNet.Infrastructure.Authorization.ImplicitFlow.ImplicitFlowVkAuthorization.GetPageType(Uri url)
   at VkNet.Infrastructure.Authorization.ImplicitFlow.ImplicitFlow.NextStepAsync(AuthorizationFormResult formResult)
   at VkNet.Infrastructure.Authorization.ImplicitFlow.ImplicitFlow.NextStepAsync(AuthorizationFormResult formResult)
   at VkNet.Infrastructure.Authorization.ImplicitFlow.ImplicitFlow.AuthorizeAsync()
   at VkNet.VkApi.BaseAuthorize(IApiAuthParams authParams)
   at VkNet.VkApi.AuthorizeWithAntiCaptcha(IApiAuthParams authParams)
   at VkNet.VkApi.Authorize(IApiAuthParams params)
   at VkNet.VkApi.Authorize(ApiAuthParams params)
   at KustarovBot.Program.Main() in C:\Users\thepu\source\repos\KustarovBot\KustarovBot\Program.cs:line 16

2. Вылезла капча при попытке отправки сообщения
   VkNet.Exception.CaptchaNeededException
  HResult=0x80131500
  Сообщение = Captcha needed
  Источник = VkNet
  Трассировка стека:
   at VkNet.Utils.VkErrors.IfErrorThrowException(String json)
   at VkNet.VkApi.Invoke(String methodName, IDictionary`2 parameters, Boolean skipAuthorization)
   at VkNet.VkApi.CallBase(String methodName, VkParameters parameters, Boolean skipAuthorization)
   at VkNet.VkApi.Call(String methodName, VkParameters parameters, Boolean skipAuthorization)
   at VkNet.Categories.MessagesCategory.Send(MessagesSendParams params)
   at VkNet.Categories.MessagesCategory.<>c__DisplayClass22_0.<SendAsync>b__0()
   at System.Threading.Tasks.Task`1.InnerInvoke()
   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.ValidateEnd(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at KustarovBot.Program.<>c__DisplayClass0_0.<<Main>b__0>d.MoveNext() in C:\Users\thepu\source\repos\KustarovBot\KustarovBot\Program.cs:line 38

  Изначально это исключение было создано в этом стеке вызовов: 
    [Внешний код]
    KustarovBot.Program.Main.AnonymousMethod__0(VkNet.Model.Message, VkNet.Model.User) в Program.cs