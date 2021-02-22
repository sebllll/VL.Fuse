﻿using System.Collections.Generic;
using Stride.Graphics;

namespace Fuse
{
    public class GetBufferNode<T> : ShaderNode<T> where T : struct
    {
        private GpuValue<Buffer> _buffer;
        private GpuValue<int> _index;
        
        public GetBufferNode(GpuValue<Buffer> theBuffer, GpuValue<int> theIndex, ConstantValue<T> theDefault) : base( "getBuffer", theDefault)
        {
            _buffer = theBuffer;
            _index = theIndex;
            
            Setup(new List<AbstractGpuValue>(){theBuffer,theIndex});
        }

        protected override string SourceTemplate()
        {
            const string shaderCode = "${resultType} ${resultName} = ${bufferName}[${index}];";
            return ShaderTemplateEvaluator.Evaluate(shaderCode,new Dictionary<string, string>()
            {
                {"bufferName", _buffer.ID},
                {"index", _index.ID}
            });
        }
    }
}