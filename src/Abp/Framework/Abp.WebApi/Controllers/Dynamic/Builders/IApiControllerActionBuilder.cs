namespace Abp.WebApi.Controllers.Dynamic.Builders
{
    /// <summary>
    /// This interface is used to define a dynamic api controller action.
    /// </summary>
    /// <typeparam name="T">Type of the proxied object</typeparam>
    public interface IApiControllerActionBuilder<T>
    {
        /// <summary>
        /// Used to specify Http verb of the action.
        /// </summary>
        /// <param name="verb">Http very</param>
        /// <returns>Action builder</returns>
        IApiControllerActionBuilder<T> WithVerb(HttpVerb verb);

        /// <summary>
        /// Used to specify name of the action.
        /// </summary>
        /// <param name="name">Action name</param>
        /// <returns></returns>
        IApiControllerActionBuilder<T> WithActionName(string name);

        /// <summary>
        /// Used to specify another method definition.
        /// </summary>
        /// <param name="methodName">Name of the method in proxied type</param>
        /// <returns>Action builder</returns>
        IApiControllerActionBuilder<T> ForMethod(string methodName);

        /// <summary>
        /// Tells builder to not create action for this method.
        /// </summary>
        IApiControllerBuilder<T> DontCreateAction();

        /// <summary>
        /// Builds the controller.
        /// This method must be called at last of the build operation.
        /// </summary>
        void Build();
    }
}