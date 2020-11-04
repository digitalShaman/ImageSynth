Imports System.Xml

Public MustInherit Class FunctionBase
    Implements IFunction

    Public MustOverride ReadOnly Property Name As String Implements IFunction.Name
    Public MustOverride ReadOnly Property Type As FunctionType Implements IFunction.Type
    Public MustOverride Function Evaluate(ByVal x As Double, ByVal y As Double) As Double Implements IFunction.Evaluate
    Public MustOverride ReadOnly Property ParameterInfos As System.Data.DataTable Implements IFunction.ParameterInfos
    Public MustOverride Sub SetParameterValue(ByVal Index As Integer, ByVal Value As Double) Implements IFunction.SetParameterValue
    Public MustOverride Sub AddParameterValue(Index As Int32, Value As Double) Implements IFunction.AddParameterValue
    Public MustOverride ReadOnly Property Help As String Implements IFunction.Help


    Property Outputs As New List(Of OutputTo) Implements IFunction.Outputs
    Property RequiredInputCount As Int32 Implements IFunction.RequiredInputCount
    Property ProvidedInputCount As Int32 Implements IFunction.ProvidedInputCount
    Property IsSolo As Boolean = False Implements IFunction.IsSolo
    Property IsMute As Boolean = False Implements IFunction.IsMute

    Property Tag As Object Implements IFunction.Tag

    Private id As Guid = Guid.Empty
    Public Property GetGuid As Guid Implements IFunction.Guid
        Get
            If id.Equals(Guid.Empty) Then
                id = Guid.NewGuid
            End If
            Return id
        End Get
        Set(ByVal value As Guid)
            id = value
        End Set
    End Property

    Public Sub SetParameterValue(ByVal Name As String, ByVal Value As Double) Implements IFunction.SetParameterValue
        Dim tbl As DataTable = ParameterInfos
        Dim row As DataRow = tbl.AsEnumerable.FirstOrDefault(Function(row1) CStr(row1("Name")) = Name)
        If row IsNot Nothing Then
            Dim iParameter As Int32 = CInt(row("Index"))
            SetParameterValue(iParameter, Value)
        End If
    End Sub


    Public ReadOnly Property ParameterValue(ByVal Index As Integer) As Double Implements IFunction.ParameterValue
        Get
            Dim tbl As DataTable = ParameterInfos
            If Index <= tbl.Rows.Count - 1 Then
                Return CDbl(tbl.Rows(Index)("Current"))
            Else
                Return 0
            End If
        End Get
    End Property


    Public Sub AddOutput(ByVal Output As OutputTo) Implements IFunction.AddOutput
        If Not Outputs.Any(Function(o) o.FromPin = Output.FromPin AndAlso o.ToPin = Output.ToPin AndAlso o.Equals(Output.ToFunction)) Then
            Dim tbl As DataTable = Output.ToFunction.ParameterInfos
            Output.ToConnectorName = CStr(tbl.Rows(Output.ToPin)("Name"))
            Outputs.Add(Output)
            If Me.Type <> FunctionType.Repeater Then
                'wenn das kein repeater ist, dann sollen alle angeschlossenen module auf den output warten
                Output.ToFunction.RequiredInputCount += 1
            End If

        End If
    End Sub

    Public Sub RemoveOutput(ByVal Output As OutputTo) Implements IFunction.RemoveOutput
        Outputs.Remove(Output)
        If Me.Type <> FunctionType.Repeater Then
            Output.ToFunction.RequiredInputCount -= 1
        End If
        Dim tbl As DataTable = Output.ToFunction.ParameterInfos
        Output.ToFunction.SetParameterValue(Output.ToPin, CDbl(tbl.Rows(Output.ToPin)("Default")))
    End Sub



    Public Sub Serialize(ByVal Parent As Xml.XmlNode) Implements IFunction.Serialize
        Dim nd As Xml.XmlElement = Parent.OwnerDocument.CreateElement("FunctionInstance")
        Parent.AppendChild(nd)

        nd.SetAttribute("Guid", GetGuid.ToString)
        nd.SetAttribute("IsSolo", If(IsSolo, 1, 0).ToString)
        nd.SetAttribute("IsMute", If(IsMute, 1, 0).ToString)
        nd.SetAttribute("CodeName", Me.GetType.ToString.Split("."c)(1))

        Dim tbl As DataTable = Me.ParameterInfos
        Dim ltNonDefaultValues As List(Of Tuple(Of String, Double)) = tbl.AsEnumerable.Where(Function(row) CDbl(row("Current")) <> CDbl(row("Default"))).
                Select(Function(row) New Tuple(Of String, Double)(CStr(row("Name")), CDbl(row("Current")))).ToList
        ltNonDefaultValues.ForEach(Sub(o)
                                       Dim ndChild As Xml.XmlElement = Parent.OwnerDocument.CreateElement("Parameter")
                                       ndChild.SetAttribute("Name", o.Item1)
                                       ndChild.SetAttribute("Value", o.Item2.ToString(System.Globalization.CultureInfo.InvariantCulture))
                                       nd.AppendChild(ndChild)
                                   End Sub)

        For Each oOutput As OutputTo In Outputs
            Dim ndOutput As Xml.XmlElement = Parent.OwnerDocument.CreateElement("Output")
            nd.AppendChild(ndOutput)
            ndOutput.SetAttribute("OutputToConnector", oOutput.ToConnectorName)
            ndOutput.SetAttribute("OutputToItem", oOutput.ToFunction.Guid.ToString)
            If Me.Type = FunctionType.Repeater Then
                ndOutput.SetAttribute("OutputFromConnector", oOutput.FromPin.ToString)
            End If
        Next

        If Me.Type = FunctionType.Repeater Then
            Dim oRep As Repeater = CType(Me, Repeater)

            Dim ndTable As Xml.XmlElement = Parent.OwnerDocument.CreateElement("DataSequenceTable")
            ndTable.InnerXml = oRep.SerializeTable
            nd.AppendChild(ndTable)

        End If
    End Sub




    Public Function Deserialize(ByVal Node As Xml.XmlNode) As IFunction

        Dim sCodeName As String = Node.Attributes("CodeName").Value

        If sCodeName.Contains(".") Then
            'patch assembly name
            sCodeName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name & "." & sCodeName.Split("."c)(1)
        Else
            sCodeName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name & "." & sCodeName
        End If
        Dim oResult As IFunction = CType(Activator.CreateInstance(Nothing, sCodeName).Unwrap, IFunction)

        oResult.Guid = Guid.Parse(Node.Attributes("Guid").Value)
        oResult.IsSolo = (Node.Attributes("IsSolo").Value = "1")
        oResult.IsMute = (Node.Attributes("IsMute").Value = "1")


        For Each nd As Xml.XmlElement In Node.SelectNodes("Parameter")
            Dim sParamName As String = nd.Attributes("Name").Value
            Dim fValue As Double = Double.Parse(nd.Attributes("Value").Value, System.Globalization.CultureInfo.InvariantCulture)
            oResult.SetParameterValue(sParamName, fValue)
        Next

        For Each nd As Xml.XmlElement In Node.SelectNodes("Output")
            Dim oOutput As New OutputTo
            oOutput.ToConnectorName = nd.Attributes("OutputToConnector").Value
            oOutput.ToFunctionId = Guid.Parse(nd.Attributes("OutputToItem").Value)
            If nd.Attributes("OutputFromConnector") IsNot Nothing Then
                oOutput.FromPin = Int32.Parse(nd.Attributes("OutputFromConnector").Value)
            End If
            oResult.Outputs.Add(oOutput)
        Next

        If oResult.Type = FunctionType.Repeater Then
            Dim oRep As Repeater = CType(oResult, Repeater)
            Dim ndTable As Xml.XmlNode = Node.SelectSingleNode("DataSequenceTable")
            If ndTable IsNot Nothing Then
                oRep.DeserializeTable(ndTable.InnerText)
            End If
        End If

        Return oResult
    End Function





    'stuff für OGL:
    Public Property OGLStorageIndex As Int32 Implements IFunction.OGLStorageIndex

    ReadOnly Property HasOpenGlSupport As Boolean Implements IFunction.HasOpenGlSupport
        Get
            Dim isResult As Boolean = False
            Dim sDummy1 As String = "", sDummy2 As String = "", sDummy3 As String = ""
            Try
                Me.AddOpenGlCode(sDummy1, sDummy2, sDummy3, False)
                isResult = True
            Catch ex As Exception

            End Try
            Return isResult
        End Get
    End Property

    Overridable Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean) Implements IFunction.AddOpenGlCode
        Throw New NotImplementedException
    End Sub

    Public Function GetOpenGlAddCode(ToConnector As Int32, ValueVariable As String) As String Implements IFunction.GetOpenGlAddCode
        Return $"Storage[{OGLStorageIndex + ToConnector}] += {ValueVariable};"
    End Function



End Class
