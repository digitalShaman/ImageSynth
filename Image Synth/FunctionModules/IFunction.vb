Public Interface IFunction
    ReadOnly Property Name As String
    ReadOnly Property Type As FunctionType
    Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
    Sub SetParameterValue(ByVal Index As Int32, ByVal Value As Double)
    Sub SetParameterValue(ByVal Name As String, ByVal Value As Double)
    Sub AddParameterValue(ByVal Index As Int32, ByVal Value As Double)
    ReadOnly Property ParameterValue(ByVal Index As Int32) As Double
    ReadOnly Property ParameterInfos() As DataTable
    ReadOnly Property Help As String
    Sub Serialize(ByVal Parent As Xml.XmlNode)

    Property Guid() As Guid
    Property Outputs As List(Of OutputTo)
    Sub AddOutput(ByVal Output As OutputTo)
    Sub RemoveOutput(ByVal Output As OutputTo)
    Property RequiredInputCount As Int32
    Property ProvidedInputCount As Int32
    Property IsSolo As Boolean
    Property IsMute As Boolean

    Property Tag As Object

    'stuff für OGL:
    Property OGLStorageIndex As Int32
    ReadOnly Property HasOpenGlSupport As Boolean
    Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean)
    Function GetOpenGlAddCode(ToConnector As Int32, ValueVariable As String) As String

End Interface

Public Enum FunctionType
    Generator
    [Function]
    Player
    Repeater
    ImageOutput
End Enum