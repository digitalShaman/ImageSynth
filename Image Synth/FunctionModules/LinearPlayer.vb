Public Class LinearPlayer
    Inherits FunctionBase

    Private fStart As Double = 0
    Private fEnd As Double = 1
    Private iFrames As Int32 = 60

    Public idOglUniform As Int32
    Public Function UpdateWorld() As Single
        Static cFrames As Int32 = 0
        Dim f As Double = cFrames / iFrames
        Dim fResult As Double = fStart * (1 - f) + fEnd * f
        cFrames = (cFrames + 1) Mod iFrames
        Return CSng(fResult)
    End Function


    Public Overrides Sub AddParameterValue(ByVal Index As Integer, ByVal Value As Double)

    End Sub

    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        Return fStart '* (1 - x) + fEnd * x
    End Function

    Public Overrides ReadOnly Property Help As String
        Get
            Return $"Erzeugt einen linearen Verlauf, von Start ({fStart:0.000}) zu End ({fEnd:0.000}) in {iFrames} Frames."
        End Get
    End Property

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Linear Player"
        End Get
    End Property

    Public Overrides ReadOnly Property ParameterInfos As System.Data.DataTable
        Get
            Dim tbl As New DataTable
            tbl.Columns.Add("Index", GetType(Int32))
            tbl.Columns.Add("Name", GetType(String))
            tbl.Columns.Add("Step", GetType(Double))
            tbl.Columns.Add("Default", GetType(Double))
            tbl.Columns.Add("Current", GetType(Double))
            tbl.Rows.Add(0, "Start", 0.1, 0, fStart)
            tbl.Rows.Add(1, "End", 0.1, 1, fEnd)
            tbl.Rows.Add(2, "Frames", 1, 60, iFrames)
            Return tbl
        End Get
    End Property

    Public Overloads Overrides Sub SetParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Select Case Index
            Case 0
                fStart = Value
            Case 1
                fEnd = Value
            Case 2
                iFrames = CInt(Value)
        End Select
    End Sub

    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Player
        End Get
    End Property








    Overrides Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean)
        DeclareCode &= GetDeclareCode()
        InitCode &= GetInitCode()
        ExecuteCode &= GetExecuteCode(AnySolo)
    End Sub

    Private Function GetDeclareCode() As String
        Return $"uniform float LinearPlayer{OGLStorageIndex};"
    End Function

    Private Function GetInitCode() As String
        Return ""
    End Function

    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Return ""
    End Function





End Class
