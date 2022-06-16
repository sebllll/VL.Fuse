﻿using System.Collections.Generic;
using System.Text;
using Stride.Engine;
using VL.Stride.Shaders.ShaderFX;

namespace Fuse.ShaderFX
{
    public class ToShaderFX<T> : AbstractToShaderFX<T> 
    {

        private const string ShaderSource = @"
shader ${shaderID} : VS_PS_Base, Texturing, Compute${shaderType}${mixins}
{
    rgroup PerMaterial{
${groupDeclarations}
    }

    cbuffer PerMaterial{
${declarations}
    }

${structs}

${functions}

${compositions}

    override ${resultType} Compute()
    {
${sourceFX}
        ${resultFX}
    }
};";

        
        
        
        public ToShaderFX(ShaderNode<T> theCompute, bool theIsCompute = false, string theShaderSource = ShaderSource) : base( 
            theCompute, 
            new List<string>(),
            new Dictionary<string, string>(),
            theIsCompute,
            theShaderSource)
        {
        }

       
    }
}