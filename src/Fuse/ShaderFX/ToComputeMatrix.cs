﻿using System.Collections.Generic;
using Fuse.compute;
using Stride.Core.Mathematics;
using Stride.Engine;
using VL.Stride.Shaders.ShaderFX;

namespace Fuse.ShaderFX
{
    public class ToComputeMatrix : AbstractToShaderFX<Matrix>  , IComputeValue<Matrix>
    {
        
        private const string ComputeShaderSource = @"
shader ${shaderID} : ComputeMatrix, ComputeShaderBase${mixins}
{

${structs}

    cbuffer PerDispatch{
${declarations}
    }

${functions}

    override float4x4 Compute()
    {
${sourceCS}
    }
};";

        public ToComputeMatrix(Game theGame, ShaderNode<Matrix> theCompute) : base(theGame, 
            theCompute, 
            new List<string>(),
            new Dictionary<string, string>(),
            true,
            ComputeShaderSource)
        {
        }
    }
}