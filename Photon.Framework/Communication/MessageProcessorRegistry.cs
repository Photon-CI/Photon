using Photon.Framework.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Photon.Framework.Communication
{
    internal class MessageProcessorRegistry
    {
        private readonly Dictionary<Type, Func<IRequestMessage, Task<IResponseMessage>>> processorMap;


        public MessageProcessorRegistry()
        {
            processorMap = new Dictionary<Type, Func<IRequestMessage, Task<IResponseMessage>>>();
        }

        public void Scan(Assembly assembly)
        {
            var classTypes = assembly.DefinedTypes
                .Where(t => t.IsClass && !t.IsAbstract);

            var typeGenericMessageProcessor = typeof(IProcessMessage<>);

            foreach (var classType in classTypes) {
                foreach (var classInterface in classType.GetInterfaces()) {
                    if (!classInterface.IsGenericType) continue;

                    var classInterfaceGenericType = classInterface.GetGenericTypeDefinition();
                    if (classInterfaceGenericType != typeGenericMessageProcessor) continue;

                    var argumentTypeList = classInterface.GetGenericArguments();
                    if (argumentTypeList.Length != 1) continue;

                    var requestType = argumentTypeList[0];

                    var method = classInterface.GetMethod("Process");
                    var genericMethod = method.MakeGenericMethod(requestType);

                    processorMap[requestType] = async request => {
                        object processor = null;

                        try {
                            processor = Activator.CreateInstance(classType);

                            var arguments = new[] {request};
                            var result = genericMethod.Invoke(processor, arguments);

                            return await (Task<IResponseMessage>)result;
                        }
                        finally {
                            (processor as IDisposable)?.Dispose();
                        }
                    };
                }
            }
        }

        public async Task<IResponseMessage> Process(IRequestMessage requestMessage)
        {
            var requestType = requestMessage.GetType();

            if (!processorMap.TryGetValue(requestType, out var processorFunc))
                throw new Exception($"No processor found matching request type '{requestType.Name}'!");

            return await processorFunc.Invoke(requestMessage);
        }
    }
}
