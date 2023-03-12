﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Fuse.compute;
using VL.Core;
using VL.Stride.Shaders.ShaderFX;

namespace Fuse
{
    public class PassThroughNode<T> : ShaderNode<T>
    {
        private ShaderNode<T> _input;

        private readonly bool _triggerChange;
        public ShaderNode<T> Input { get => _input;
            set
            {
                _input = value ?? Default;
                SetInputs(new List<AbstractShaderNode>{Input}, _triggerChange);
            }
        }
        
        public PassThroughNode(NodeContext nodeContext, string theName = "PassThrough", bool triggerChange = true) : base(nodeContext, theName)
        {
            Default = new ShaderNode<T>(new NodeSubContextFactory(NodeContext).NextSubContext(),"PassThroughDefault");
            _triggerChange = triggerChange;
            // ReSharper disable once VirtualMemberCallInConstructor
            Input = Default;
        }
        
        public PassThroughNode(NodeContext nodeContext, string theName, AbstractShaderNode theValue, bool triggerChange = true) : this(nodeContext, theName, triggerChange)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            SetInput(theValue);
        }
        
        protected override string SourceTemplate()
        {
            return "";
        }
        
        protected override string GenerateDefaultSource()
        {
            return "";
        }

        public virtual void SetInput(AbstractShaderNode theValue)
        {
            Input = theValue as ShaderNode<T>;
        }
        public override string ID => _input.ID;
        
        public override string TypeName()
        {
            return Input != null ? Input.TypeName() : base.TypeName();
        }
    }
    
    public class PassThroughDelegate<T> : Delegate<T>
    {
        private Delegate<T> _input;

        private readonly bool _triggerChange;
        public Delegate<T> Input { get => _input;
            set
            {
                _input = value;
                SetInputs(new List<AbstractShaderNode>{Input}, _triggerChange);
            }
        }
        
        public PassThroughDelegate(NodeContext nodeContext, string theName = "PassThrough", bool triggerChange = true) : base(nodeContext, theName)
        {
            Default = new ShaderNode<T>(new NodeSubContextFactory(NodeContext).NextSubContext(),"PassThroughDefault");
            _triggerChange = triggerChange;
            // ReSharper disable once VirtualMemberCallInConstructor
        }
        
        public PassThroughDelegate(NodeContext nodeContext, string theName, AbstractShaderNode theValue, bool triggerChange = true) : this(nodeContext, theName, triggerChange)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            SetInput(theValue);
        }
        
        protected override string SourceTemplate()
        {
            return "";
        }
        
        protected override string GenerateDefaultSource()
        {
            return "";
        }

        public virtual void SetInput(IDelegate theValue)
        {
            Input = theValue as Delegate<T>;
        }
        public override string ID => _input.ID;
        
        public override string TypeName()
        {
            return Input != null ? Input.TypeName() : base.TypeName();
        }
    }

    public class PassVoid : PassThroughNode<GpuVoid>, IComputeVoid
    {
        public PassVoid(NodeContext nodeContext, string theName = "PassThrough") : base(nodeContext, theName)
        {
        }

        public PassVoid(NodeContext nodeContext, string theName, AbstractShaderNode theValue) : base(nodeContext, theName, theValue)
        {
        }
    }

    public class Output<T> : ShaderNode<T>
    {

        private AbstractShaderNode _input;
        public Output(NodeContext nodeContext, ShaderNode<T> theDefault = null) : base(nodeContext, "AddOutput", theDefault)
        {
            
        }

        public void SetInputs(AbstractShaderNode theComputation, ShaderNode<T> theValue)
        {
            _input = theValue;
            SetInputs(new List<AbstractShaderNode>{theComputation, theValue});
        }
        
        public void SetInputsAbstract(AbstractShaderNode theComputation, AbstractShaderNode theValue)
        {
            _input = theValue;
            SetInputs(new List<AbstractShaderNode>{theComputation, theValue});
        }
        
        protected override string SourceTemplate()
        {
            return "";
        }
        
        protected override string GenerateDefaultSource()
        {
            return "";
        }
        
        public override string ID => _input.ID;
    }
    
    public class CrossLink<T> : PassThroughNode<T>
    {

        public CrossLink(NodeContext nodeContext) : base(nodeContext, "CrossLink")
        {
        }
    }
    
    public class Comment<T> : PassThroughNode<T>
    {

        private string _comment;
        
        public Comment(NodeContext nodeContext) : base(nodeContext, "Comment")
        {
            _comment = "";
        }

        public void SetInput(string theComment, ShaderNode<T> theIn)
        {
            _comment = theComment;
            base.SetInput(theIn);
        }

        protected override string SourceTemplate()
        {
            const string shaderCode = @"
        // ${comment}
        ";
            return ShaderNodesUtil.Evaluate(shaderCode,new Dictionary<string, string>()
            {
                {"comment", _comment}
            });
        }
    }
    
    public class Do<T> : PassThroughNode<T>
    {

        
        public Do(NodeContext nodeContext) : base(nodeContext, "Do")
        {
        }

        public void SetInput(ShaderNode<T> theIn, ShaderNode<GpuVoid> theCommand)
        {
            base.SetInput(theIn);
            
            SetInputs(new List<AbstractShaderNode>{theCommand,Input});
        }
    }
    
    public class AddProperty<T> : PassThroughNode<T>
    {
        private readonly string _propertyId;
        public AddProperty(NodeContext nodeContext, string thePropertyId) : base(nodeContext, "AddProperty")
        {
            _propertyId = thePropertyId;
        }

        public void SetProperties(IList theResources)
        {
            AddProperties(_propertyId, theResources);
            CallChangeEvent();
        }
    }
    
    public interface IInjectToRegion{}
    
    public class InjectToRegion<T> : PassThroughNode<T>, IInjectToRegion
    {
        public InjectToRegion(NodeContext nodeContext) : base(nodeContext, "InjectToRegion")
        {
        }
    }
    
    public class InjectToRegionDelegate<T> : PassThroughDelegate<T>, IInjectToRegion
    {
        public InjectToRegionDelegate(NodeContext nodeContext) : base(nodeContext, "InjectToRegion")
        {
        }
    }
    
    public class AddToGraph<T>: ShaderNode<T>, IComputeVoid
    {
        private ShaderNode<T> _input;
        public AddToGraph(NodeContext nodeContext, string name = "AddToGraph") : base(nodeContext, name)
        {
            NullInputMode = HandleNullInputMode.Remove;
        }

        public void SetInput(ShaderNode<T> theMain, IEnumerable<AbstractShaderNode> theInputs, bool theCallChangeEvent = true)
        {
            _input = theMain;
            var ins = new List<AbstractShaderNode>() { theMain };
            ins.AddRange(theInputs);
            SetInputs(ins, theCallChangeEvent);
        }

        protected override string SourceTemplate()
        {
            return "";
        }
        
        protected override string GenerateDefaultSource()
        {
            return "";
        }
        
        public override string ID => _input.ID;
    }
}