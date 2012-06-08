module TaskUtil
open System.Threading.Tasks;
open System.Threading;

type TaskHelper =
    static member Create<'T>(mainLogic,(action:System.Action<Task<'T>>))  =
        Task.Factory.StartNew(mainLogic).ContinueWith(action, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext())

    static member Create((mainLogic:System.Action),(action:System.Action<Task>)) =
        Task.Factory.StartNew(mainLogic).ContinueWith(action,CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext())    

    static member Create((mainLogic:System.Action<obj>),(action:System.Action<Task>),(state:obj))  =
        Task.Factory.StartNew(mainLogic,state).ContinueWith(action,CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext())

    static member Create<'T>(mainLogic,(action:System.Action<Task<'T>>),(state:obj)) =
        Task.Factory.StartNew<'T>(mainLogic,state).ContinueWith(action,CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext())