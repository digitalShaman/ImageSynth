Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports OpenTK
Imports OpenTK.Graphics.ES30
Imports OpenTK.Input

Public Class Scene
    Inherits GameWindow

    Private Const Z_NEAR As Single = 0.2F
    Private Const Z_FAR As Single = 1000
    Private Const CULLING As CullFaceMode = CullFaceMode.Back

    Public CameraPos As New Vector3(0, 0, -2)
    Private mCamera As Matrix4 = Matrix4.LookAt(0, 0, 0, 0, 1, 0, 0, 0, 1) ';  // -> stehe 0,0,0 schaue nach norden (y+), kamera up richtung oben (z+)'(0, -2, 1, 0, 0, 1, 0, 0, 1) 'neutral
    Dim mPerspective As Matrix4


    Private idVertexBuffer As Int32
    Private idElementBuffer As Int32

    Private idOpenGlProgram As Int32
    Private idUniformProjectionMatrix As Int32  'ids für die uniforms in den shadern

    Private ltPlayers As List(Of LinearPlayer)

    Private oModel As ModelInstance

    Public Function UploadScene(Synth As ImageSynth) As Boolean
        CleanUp()

        GL.ClearColor(0.1F, 0.1F, 0.1F, 1)

        GL.ClearDepth(1.0F)
        GL.Enable(EnableCap.DepthTest)
        'GL.DepthFunc(DepthFunction.Lequal) '<-less=default

        If CULLING = 0 Then
            GL.Disable(EnableCap.CullFace)
        Else
            GL.Enable(EnableCap.CullFace)
            GL.CullFace(CULLING)
        End If
        'GL.Enable(EnableCap.Blend)
        'GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha)

        If (GL.GetError <> ErrorCode.NoError) Then Stop

        Dim sFragShader As String = ""

        Dim sError As String = ""
        sError = CompileOGL(Synth, sFragShader)
        If sError = "OK" Then sError = Compile(sVertexShaderSource, sFragShader, idOpenGlProgram)
        If Not sError = "OK" Then
            CleanUp()
            MessageBox.Show(sError, "Fehler:", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return False
        End If

        'ReadUniformIds:
        idUniformProjectionMatrix = GL.GetUniformLocation(idOpenGlProgram, "projection")
        ltPlayers = New List(Of LinearPlayer)
        For Each oPlayer As LinearPlayer In Synth.Functions.Where(Function(o) o.GetType = GetType(LinearPlayer)).ToList
            oPlayer.idOglUniform = GL.GetUniformLocation(idOpenGlProgram, $"LinearPlayer{oPlayer.OGLStorageIndex}")
            ltPlayers.Add(oPlayer)
        Next


        If (GL.GetError <> ErrorCode.NoError) Then Stop

        oModel = New ModelInstance
        oModel.Location = New Vector3(0, 1.5, 2)

        Dim hVertexBuffer As GCHandle = GCHandle.Alloc(oModel.VertexBuffer, GCHandleType.Pinned)
        GL.GenBuffers(1, idVertexBuffer)
        GL.BindBuffer(BufferTarget.ArrayBuffer, idVertexBuffer)
        GL.BufferData(BufferTarget.ArrayBuffer, oModel.VertexBuffer.Length * cbSingle,
                      hVertexBuffer.AddrOfPinnedObject, BufferUsageHint.StaticDraw)
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0)
        hVertexBuffer.Free()

        Dim hElementBuffer As GCHandle = GCHandle.Alloc(oModel.ElementBuffer, GCHandleType.Pinned)
        GL.GenBuffers(1, idElementBuffer)
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, idElementBuffer)
        GL.BufferData(BufferTarget.ElementArrayBuffer, oModel.ElementBuffer.Length * cbUShort,
                      hElementBuffer.AddrOfPinnedObject, BufferUsageHint.StaticDraw)
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0)
        hElementBuffer.Free()

        If (GL.GetError <> ErrorCode.NoError) Then Stop
        Return True
    End Function



    Private Function CompileOGL(Synth As ImageSynth, ByRef ShaderCode As String) As String
        Dim isError As Boolean = False
        Dim ltPath As List(Of IFunction) = Synth.GetFunctionPath(isError)
        If isError Then Stop    'kann keinen pfad finden! (zirkelschluss?)

        Dim ltOutputs As List(Of OutputTo) = ltPath.SelectMany(Function(o) o.Outputs).ToList
        ltOutputs.ForEach(Sub(o) o.ZeroValue())
        If Synth.Functions.Any(Function(o) o.IsSolo) Then
            'bei solo muss imgout explizit hinzugefügt werden
            If ltPath.Any(Function(o) o.Type = FunctionType.ImageOutput) Then Stop 'sanity check
            Dim oImageOut As ImageOutput = Synth.Functions.OfType(Of ImageOutput).First
            oImageOut.SetParameterValue(0, 0)
            oImageOut.SetParameterValue(1, 0)
            oImageOut.SetParameterValue(2, 0)
            ltPath.Add(oImageOut)
        End If

        'ids zuweisen:
        Synth.Functions.ForEach(Sub(o) o.OGLStorageIndex = -1)
        Dim iFunction As Int32 = 0
        Dim cPlayers As Int32 = 0
        For Each oFunc As IFunction In ltPath
            If oFunc.Type = FunctionType.Player Then
                oFunc.OGLStorageIndex = cPlayers
                cPlayers += 1
            Else
                oFunc.OGLStorageIndex = iFunction
                iFunction += oFunc.ParameterInfos.Rows.Count
            End If
        Next

        Dim sFuncArrayDeclareShaderCode As String = ""
        Dim sInitShaderCode As String = ""
        Dim sExecuteShaderCode As String = ""

        'dimensionierung des storage für die funktionen. dort liegen die input 'pins' der functions:
        sFuncArrayDeclareShaderCode = $"float Storage[{iFunction}];"

        Dim oSoloInstance As IFunction = Synth.Functions.FirstOrDefault(Function(o) o.IsSolo)
        Dim isAnySolo As Boolean = oSoloInstance IsNot Nothing
        Dim sPoints As String = "", cPoints As Int32 = 0
        For Each oFunc As IFunction In ltPath
            Try
                oFunc.AddOpenGlCode(sFuncArrayDeclareShaderCode, sInitShaderCode, sExecuteShaderCode, isAnySolo)
            Catch ex As Exception
                Return $"Das Modul {oFunc.Name} hat keine Open GL Implementierung!"
            End Try
            If oFunc.GetType = GetType(Voronoi) Then
                'ugly pt[] hack
                sPoints &= CType(oFunc, Voronoi).GetPointCode()
                sInitShaderCode &= $"Storage[{oFunc.OGLStorageIndex + 3}] = {cPoints}.0;"    'start index
                cPoints = sPoints.Split({"vec2"}, StringSplitOptions.None).Count - 1
            End If

            If oFunc.IsSolo Then
                'output zu image out hinzufügen
                Dim oImgOut As ImageOutput = ltPath.OfType(Of ImageOutput).First
                sExecuteShaderCode &= oImgOut.GetOpenGlAddCode(0, $"value{oFunc.OGLStorageIndex}")
                sExecuteShaderCode &= oImgOut.GetOpenGlAddCode(1, $"value{oFunc.OGLStorageIndex}")
                sExecuteShaderCode &= oImgOut.GetOpenGlAddCode(2, $"value{oFunc.OGLStorageIndex}")
            End If
        Next

        If cPoints > 0 Then
            sFuncArrayDeclareShaderCode = $"const vec2 VoronoiPoints[{cPoints}] = vec2[{cPoints}]({sPoints.Substring(1)});" & sFuncArrayDeclareShaderCode
        Else
            sFuncArrayDeclareShaderCode &= $"vec2 VoronoiPoints[1];"
        End If

        For Each oFunc As LinearPlayer In ltPath.OfType(Of LinearPlayer)
            For Each oChild As OutputTo In oFunc.Outputs.Where(Function(o) o.ToFunction.OGLStorageIndex <> -1)
                Dim oFuncTo As FunctionBase = CType(oChild.ToFunction, FunctionBase)

                sInitShaderCode &= oFuncTo.GetOpenGlAddCode(oChild.ToPin, $"LinearPlayer{oFunc.OGLStorageIndex}")
            Next
        Next

        Dim sResult As String = IO.File.ReadAllText("OGLRendering\FragmentShader.txt")
        sResult = sResult.Replace("/*DECLARE_FUNCTIONS*/", sFuncArrayDeclareShaderCode)
        sResult = sResult.Replace("/*INIT_FUNCTIONS*/", sInitShaderCode)
        sResult = sResult.Replace("/*EXECUTE_FUNCTIONS*/", sExecuteShaderCode)
        ShaderCode = sResult
        Return "OK"
    End Function







    Public Sub CleanUp()
        If idVertexBuffer <> 0 Then
            GL.DeleteBuffers(2, {idVertexBuffer, idElementBuffer})
            GL.DeleteProgram(idOpenGlProgram)
            If GL.GetError() <> ErrorCode.NoError Then Stop
            idVertexBuffer = 0 : idElementBuffer = 0
            oModel = Nothing
        End If
    End Sub

    Protected Overrides Sub OnRenderFrame(e As FrameEventArgs)
        MyBase.OnRenderFrame(e)

        GL.Clear(ClearBufferMask.ColorBufferBit Or ClearBufferMask.DepthBufferBit)

        Dim R As Matrix4 = GetGameRotation()
        Dim View As Matrix4 = Matrix4.Mult(R, mCamera)
        Dim mCameraPos As Matrix4 = Matrix4.CreateTranslation(CameraPos)
        View = Matrix4.Mult(mCameraPos, View)
        Dim PxV As Matrix4 = Matrix4.Mult(View, mPerspective) '; // Perspective * View

        GL.Viewport(0, 0, Width, Height)
        DrawScene(PxV)

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0)
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0)
        If GL.GetError() <> ErrorCode.NoError Then Stop
        SwapBuffers()
    End Sub

    Private Sub DrawScene(PxV As Matrix4)
        GL.UseProgram(idOpenGlProgram)

        Dim err As ErrorCode = GL.GetError
        If err = ErrorCode.OutOfMemory Then
            MessageBox.Show("OpenGL Out of Memory Error. Zu viele Voronoi Punkte?")
            CleanUp()
        End If

        For Each oPlayer As LinearPlayer In ltPlayers
            GL.Uniform1(oPlayer.idOglUniform, oPlayer.UpdateWorld)
        Next

        oModel.UpdateWorld()
        'SetProjection
        Dim M As Matrix4 = oModel.ModelMatrix()
        Dim PxVxM As Matrix4 = Matrix4.Mult(M, PxV) ' // Perspective * View * Model
        GL.UniformMatrix4(idUniformProjectionMatrix, False, PxVxM)

        GL.BindBuffer(BufferTarget.ArrayBuffer, idVertexBuffer)
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, idElementBuffer)


        Dim cbStride As Int32 = oModel.VertexInfos.Select(Function(info) info.Size).Sum
        Dim cOffset As Int32 = 0
        For i As Int32 = 0 To oModel.VertexInfos.Count - 1
            Dim info As ModelInstance.VertexInfo = oModel.VertexInfos(i)
            GL.VertexAttribPointer(i, info.Count, info.Type, False, cbStride, cOffset)
            GL.EnableVertexAttribArray(i)
            cOffset += info.Size
        Next

        GL.DrawElements(PrimitiveType.Triangles, oModel.ElementBuffer.Count,
                                        DrawElementsType.UnsignedShort, IntPtr.Zero)
        If GL.GetError() <> ErrorCode.NoError Then Stop
    End Sub

    Private Sub Scene_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Dim fAspectRatio As Single = CSng(Width / Height)
        mPerspective = Matrix4.CreatePerspectiveFieldOfView(OpenTK.MathHelper.DegreesToRadians(42.0F), fAspectRatio, Z_NEAR, Z_FAR)
    End Sub


    Private Function Compile(vertShaderSource As String, fragShaderSource As String, ByRef OutProgramId As Int32) As String
        Dim vertShader, fragShader As Int32
        '// Create shader program.
        OutProgramId = GL.CreateProgram()

        Dim sErrorText As String = ""
        Dim sCompileResult As String
        sCompileResult = CompileShader(ShaderType.VertexShader, vertShaderSource, vertShader)
        If (sCompileResult <> "OK") Then
            sErrorText = "vsh error: " + sCompileResult '    //Error loading vertex shader
        End If
        sCompileResult = CompileShader(ShaderType.FragmentShader, fragShaderSource, fragShader)
        If (sCompileResult <> "OK") Then
            sErrorText &= "fsh error: " + sCompileResult '    //Error loading fragment shader
        End If

        If sErrorText = "" Then
            '// Attach shaders to program.
            GL.AttachShader(OutProgramId, vertShader)
            GL.AttachShader(OutProgramId, fragShader)

            '// Link program.
            GL.LinkProgram(OutProgramId)
            Dim linked As Int32 = 0
            GL.GetProgram(OutProgramId, GetProgramParameterName.LinkStatus, linked)
            sErrorText = If(linked = 0, "Failed to link program" + GL.GetProgramInfoLog(OutProgramId), "")
        End If

        If sErrorText = "" Then
            '//// Release vertex And fragment shaders.
            GL.DetachShader(OutProgramId, vertShader)
            GL.DeleteShader(vertShader)
            vertShader = 0
            GL.DetachShader(OutProgramId, fragShader)
            GL.DeleteShader(fragShader)
            fragShader = 0

            If (GL.GetError <> ErrorCode.NoError) Then
                sErrorText = "GL Error"
            End If

        End If

        If sErrorText <> "" Then
            If (vertShader <> 0) Then
                GL.DeleteShader(vertShader)
            End If
            If (fragShader <> 0) Then
                GL.DeleteShader(fragShader)
            End If
            If (OutProgramId <> 0) Then
                GL.DeleteProgram(OutProgramId)
                OutProgramId = 0
            End If
            Return sErrorText
        Else
            Return "OK"
        End If
    End Function

    Private Function CompileShader(type As ShaderType, src As String, ByRef outShaderId As Int32) As String

        outShaderId = GL.CreateShader(type)
        If (outShaderId = 0) Then
            Return ("CreateShader failed")
        End If

        GL.ShaderSource(outShaderId, src)
        GL.CompileShader(outShaderId)

        Dim status As Int32 = 0
        GL.GetShader(outShaderId, ShaderParameter.CompileStatus, status)
        If (status = 0) Then
            '//Not compiled
            Dim sLog As String = "Error"
            GL.GetShader(outShaderId, ShaderParameter.InfoLogLength, status)
            If (status > 0) Then
                sLog = GL.GetShaderInfoLog(outShaderId)
            End If
            GL.DeleteShader(outShaderId)
            Return sLog
        End If

        Return "OK"
    End Function

    Private Sub Scene_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        CleanUp()
    End Sub


#Region "Shader Code"
    Private sVertexShaderSource As String = "#version 300 es" & Environment.NewLine &
        "" &
        "in vec4 position;" &
        "in vec2 texcoord;" &
        "" &
        "out vec2 textureCoordinate;" &
        "" &
        "uniform mat4 projection;" &
        "" &
        "void main()" &
        "{" &
        "   gl_Position = projection * position;" &
        "   textureCoordinate = texcoord;" +
        "}"



#End Region
End Class

