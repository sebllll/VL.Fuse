﻿using System;
using System.Collections.Generic;
using System.Text;
using Stride.Core.Extensions;
using Stride.Graphics;
using VL.Stride.Shaders.ShaderFX;

namespace Fuse.compute
{
    
    public abstract class AbstractTypedFunction<TIn,TOut>: ShaderNode<TOut> where TIn : struct
    {
        

        protected AbstractTypedFunction(string theName, ShaderNode<TOut> theDefault = null) : base( theName, theDefault)
        {
            AddResource(Structs, BuildStructs());
        }

        private static string BuildStructs()
        {
            if (!TypeHelpers.IsStructType<TIn>()) return "";
            
            const string shaderCode = 
                @"    struct ${structName}{
${structMembers}
    };" ;
            
            var call = new StringBuilder();
            typeof(TIn).GetProperties().ForEach(property =>
            {
                call.Append("        "+TypeHelpers.GetGpuTypeForType(property.PropertyType) + " " + property.Name+";"+Environment.NewLine);
            });
            return ShaderNodesUtil.Evaluate(shaderCode,new Dictionary<string, string>()
            {
                {"structName", typeof(TIn).Name},
                {"structMembers", call.ToString()}
            });
        }
    }
    
    public class DeclareStructVariable<T>: AbstractTypedFunction<T,T> where T : struct
    {
        
        public DeclareStructVariable(ConstantValue<T> theDefault) : base( "getBuffer")
        {
            SetInputs(new List<AbstractShaderNode>(){});
        }

        protected override string SourceTemplate()
        {
            const string shaderCode = "${resultType} ${resultName};";
            return ShaderNodesUtil.Evaluate(shaderCode,new Dictionary<string, string>());
        }
    }
    
    public class TypedBufferGet<T> : AbstractTypedFunction<T,T> where T : struct
    {
        private readonly ShaderNode<Buffer<T>> _buffer;
        private readonly ShaderNode<int> _index;
        
        public TypedBufferGet(ShaderNode<Buffer<T>> theBuffer, ShaderNode<int> theIndex, ShaderNode<T> theDefault) : base( "getBuffer", theDefault)
        {
            _buffer = theBuffer;
            _index = theIndex;
            SetInputs(new List<AbstractShaderNode>(){theBuffer,theIndex});
        }

        protected override string SourceTemplate()
        {
            const string shaderCode = "${resultType} ${resultName} = ${bufferName}[${index}];";
            return ShaderNodesUtil.Evaluate(shaderCode,new Dictionary<string, string>()
            {
                {"bufferName", _buffer.ID},
                {"index", _index.ID}
            });
        }
    }
    
    public class TypedBufferSet<TIn> : AbstractTypedFunction<TIn, TIn>, IComputeVoid where TIn : struct
    {
        private readonly ShaderNode<Buffer<TIn>> _buffer;
        private readonly ShaderNode<int> _index;
        private readonly ShaderNode<TIn> _value;
    
        public TypedBufferSet(ShaderNode<Buffer<TIn>> theBuffer, ShaderNode<int> theIndex, ShaderNode<TIn> theValue) : base( "setBuffer")
        {
            _buffer = theBuffer;
            _index = theIndex;
            _value = theValue;
            
            SetInputs(new List<AbstractShaderNode>(){theBuffer,theIndex,theValue});
        }
        
        protected override Dictionary<string, string> CreateTemplateMap()
        {
            return new Dictionary<string, string>();
        }
        
        protected override string GenerateDefaultSource()
        {
            return "";
        }

        protected override string SourceTemplate()
        {
            const string shaderCode = "${bufferName}[${index}] = ${value};";
            return ShaderNodesUtil.Evaluate(shaderCode,new Dictionary<string, string>()
            {
                {"bufferName", _buffer.ID},
                {"index", _index.ID},
                {"value", _value.ID}
            });
        }
    }
    
    public class TypedBufferAppend<T> : AbstractTypedFunction<T, GpuVoid> where T : struct
    {
        private readonly ShaderNode<Buffer<T>> _buffer;
        private readonly ShaderNode<T> _value;
    
        public TypedBufferAppend(ShaderNode<Buffer<T>> theBuffer, ShaderNode<T> theValue) : base( "appendBuffer")
        {
            _buffer = theBuffer;
            _value = theValue;
            
            SetInputs(new List<AbstractShaderNode>(){theBuffer,theValue});
        }
        
        protected override Dictionary<string, string> CreateTemplateMap()
        {
            return new Dictionary<string, string>();
        }
        
        protected override string GenerateDefaultSource()
        {
            return "";
        }

        protected override string SourceTemplate()
        {
            const string shaderCode = "${bufferName}.Append(${value});";
            return ShaderNodesUtil.Evaluate(shaderCode,new Dictionary<string, string>()
            {
                {"bufferName", _buffer.ID},
                {"value", _value.ID}
            });
        }
    }
    
    public class TypedBufferConsume<T> : AbstractTypedFunction<T,T> where T : struct
    {
        private readonly ShaderNode<Buffer<T>> _buffer;
        
        public TypedBufferConsume(ShaderNode<Buffer<T>> theBuffer, ShaderNode<T> theDefault) : base( "consumeBuffer", theDefault)
        {
            _buffer = theBuffer;
            SetInputs(new List<AbstractShaderNode>(){theBuffer});
        }

        protected override string SourceTemplate()
        {
            const string shaderCode = "${resultType} ${resultName} = ${bufferName}.Consume();";
            return ShaderNodesUtil.Evaluate(shaderCode,new Dictionary<string, string>()
            {
                {"bufferName", _buffer.ID}
            });
        }
    }
}