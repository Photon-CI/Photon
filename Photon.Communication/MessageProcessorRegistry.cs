using Photon.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Photon.Communication
{
    internal delegate Task<IResponseMessage> ProcessEvent(MessageTransceiver transceiver, IRequestMessage message);

    public class MessageProcessorRegistry
    {
        private readonly Dictionary<Type, ProcessEvent> processorMap;


        public MessageProcessorRegistry()
        {
            processorMap = new Dictionary<Type, ProcessEvent>();
        }

        public void Scan(Assembly assembly)
        {
            var classTypes = assembly.DefinedTypes
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var classType in classTypes)
                Register(classType);
        }

        internal bool TryGet(Type messageType, out ProcessEvent processFunc)
        {
            return processorMap.TryGetValue(messageType, out processFunc);
        }

        public void Register<TProcessor, TRequest>()
            where TProcessor : IProcessMessage<TRequest>
            where TRequest : IRequestMessage
        {
            Register(typeof(TProcessor));
        }

        public void Register(Type processorClassType)
        {
            var typeGenericMessageProcessor = typeof(IProcessMessage<>);

            foreach (var classInterface in processorClassType.GetInterfaces()) {
                if (!classInterface.IsGenericType) continue;

                var classInterfaceGenericType = classInterface.GetGenericTypeDefinition();
                if (classInterfaceGenericType != typeGenericMessageProcessor) continue;

                var argumentTypeList = classInterface.GetGenericArguments();
                if (argumentTypeList.Length != 1) continue;

                var requestType = argumentTypeList[0];

                var method = classInterface.GetMethod("Process");
                if (method == null) continue;

                processorMap[requestType] = CreateFunc(processorClassType, method);
            }
        }
        
        private static ProcessEvent CreateFunc(Type processorClassType, MethodInfo processMethod)
        {
            return async (transceiver, message) => {
                object processorObj = null;

                try {
                    processorObj = Activator.CreateInstance(processorClassType);

                    if (!(processorObj is IProcessMessage processor))
                        throw new ApplicationException($"Invalid message processor type '{processorClassType.Name}'!");

                    processor.Transceiver = transceiver;

                    var arguments = new object[] {message};
                    var result = processMethod.Invoke(processorObj, arguments);

                    return await (Task<IResponseMessage>)result;
                }
                finally {
                    (processorObj as IDisposable)?.Dispose();
                }
            };
        }
    }
}
