Imports System.Globalization

Public Class Concentric
    Inherits FunctionBase

    Private fAdd As Double = 0
    Private fIntensity As Double = 1

    Private fRadiusMin As Double = 0
    Private fRadiusMax As Double = 0.8
    Private fFrequency As Double = 400
    Private xScale As Double = 1
    Private yScale As Double = 1
    Private xLocation As Double = 0
    Private yLocation As Double = 0

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Concentric"
        End Get
    End Property

    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Generator
        End Get
    End Property


    Public Overrides ReadOnly Property ParameterInfos As DataTable
        Get
            Dim tbl As New DataTable
            tbl.Columns.Add("Index", GetType(Int32))
            tbl.Columns.Add("Name", GetType(String))
            tbl.Columns.Add("Step", GetType(Double))
            tbl.Columns.Add("Default", GetType(Double))
            tbl.Columns.Add("Current", GetType(Double))
            tbl.Rows.Add(0, "Add", 0.1, 0, fAdd)
            tbl.Rows.Add(1, "Intensity", 0.1, 1, fIntensity)

            tbl.Rows.Add(2, "Min.Radius", 0.1, 0, fRadiusMin)
            tbl.Rows.Add(3, "Max.Radius", 0.1, 0.8, fRadiusMax)
            tbl.Rows.Add(4, "Frequency", 0.1, 400, fFrequency)
            tbl.Rows.Add(5, "Scale X", 0.1, 1, xScale)
            tbl.Rows.Add(6, "Scale Y", 0.1, 1, yScale)
            tbl.Rows.Add(7, "Location X", 0.1, 0, xLocation)
            tbl.Rows.Add(8, "Location Y", 0.1, 0, yLocation)
            Return tbl
        End Get
    End Property

    Public Overrides ReadOnly Property Help As String
        Get
            Return String.Format("Konzentrische Kreise in wählbarer Frequenz. Ab dem Min. Radius steigt die Amplitude bis zu " &
                "(MinRadius + MaxRadius)/2 auf 1.0 an und fällt danach bis Max. Radius wieder auf 0. {0}" &
                                "{0}" &
                "f = Distanz von (x + xLocation - 0.5) * xScale, (y + yLocation - 0.5) * yScale zu 0/0 {0}" &
                "fPeakDistance = (fMinRadius + fMaxRadius) / 2 {0}" &
                "fPeakWidth = fPeakDistance - fRadiusMin {0}" &
                "{0}" &
                "Result = (({0}" &
                "   Wenn MinRadius < f < MaxRadius {0}" &
                "       Math.Sin(f * Frequency) {0}" &
                "       * {0}" &
                "       (1 - (Abs(f - fPeakDistance) / fPeakWidth)) {0}" &
                "   sonst {0}" &
                "       0 {0}" &
                ") + Add ) * Intensity" &
                "{0}" &
                "", Environment.NewLine)
        End Get
    End Property

    Public Overrides Sub SetParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fAdd = Value
            Case 1
                fIntensity = Value
            Case 2
                fRadiusMin = Value
            Case 3
                fRadiusMax = Value
            Case 4
                fFrequency = Value
            Case 5
                xScale = Value
            Case 6
                yScale = Value
            Case 7
                xLocation = Value
            Case 8
                yLocation = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fAdd = Value
            Case 1
                fIntensity += Value
            Case 2
                fRadiusMin += Value
            Case 3
                fRadiusMax += Value
            Case 4
                fFrequency += Value
            Case 5
                xScale += Value
            Case 6
                yScale += Value
            Case 7
                xLocation += Value
            Case 8
                yLocation += Value
        End Select
    End Sub


    Public Overrides Function Evaluate(x As Double, y As Double) As Double
        Static fPeakDistance As Double
        Static fPeakWidth As Double
        If x = 0 AndAlso y = 0 Then
            fPeakDistance = (fRadiusMax + fRadiusMin) / 2
            fPeakWidth = (fPeakDistance - fRadiusMin)
        End If

        x = (x - 0.5) * xScale + xLocation
        y = (y - 0.5) * yScale + yLocation


        'x = (x + xLocation - 0.5) * xScale
        'y = (y + yLocation - 0.5) * yScale


        Dim fDist As Double = Math.Sqrt(x * x + y * y)
        Dim fResult As Double
        If fDist < fRadiusMin OrElse fDist > fRadiusMax Then
            fResult = 0
        Else
            fResult = Math.Sin(fDist * fFrequency)
            Dim fFactor As Double = 1 - (Math.Abs(fDist - fPeakDistance) / fPeakWidth)
            fResult *= fFactor
        End If
        Return (fResult + fAdd) * fIntensity
    End Function







    Overrides Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean)
        If Not DeclareCode.Contains("float EvaluateConcentric(int i, vec2 Location)") Then
            DeclareCode &= GetDeclareCode()
        End If
        InitCode &= GetInitCode()
        ExecuteCode &= GetExecuteCode(AnySolo)
    End Sub

    Private Function GetDeclareCode() As String
        Return "
float EvaluateConcentric(int i, vec2 Location) {
    float x = (Location.x - 0.5) * Storage[i + 5] + Storage[i + 7];
    float y = (Location.y - 0.5) * Storage[i + 6] + Storage[i + 8];
    float fDist = sqrt(x*x + y*y);
    float fResult;
    if (fDist < Storage[i + 2] || fDist > Storage[i + 3]) {
        fResult = 0.0;
    } else {
        fResult = sin(fDist * Storage[i + 4]);
        float fPeakDistance = (Storage[i + 2] + Storage[i + 3]) / 2.0;
        float fPeakWidth = (fPeakDistance - Storage[i + 2]);
        float fFactor = 1.0 - (abs(fDist - fPeakDistance) / fPeakWidth);
        fResult *= fFactor;
    }
    return (fResult + Storage[i + 0]) * Storage[i + 1];
}"
    End Function

    Private Function GetInitCode() As String
        Dim sb As New Text.StringBuilder
        Dim sFormat As String = "0.0######"
        sb.Append($"Storage[{OGLStorageIndex + 0}]={fAdd.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 1}]={fIntensity.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 2}]={fRadiusMin.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 3}]={fRadiusMax.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 4}]={fFrequency.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 5}]={xScale.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 6}]={yScale.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 7}]={xLocation.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 8}]={yLocation.ToString(sFormat, CultureInfo.InvariantCulture)};")

        Return sb.ToString
    End Function

    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Dim sTempVarName As String = $"value{OGLStorageIndex}"
        Dim sExecuteCode As String = $"float {sTempVarName}=EvaluateConcentric({OGLStorageIndex}, textureCoordinate);"

        'Ergebnis an folge funktionen weitergeben
        For Each oChild As OutputTo In Outputs
            If Not (AnySolo AndAlso oChild.ToFunction.Type = FunctionType.ImageOutput) AndAlso oChild.ToFunction.OGLStorageIndex <> -1 Then
                'aber nur, wenn kein solo oder wenn solo dann aber nicht an imageoutput
                Dim oFunc As FunctionBase = CType(oChild.ToFunction, FunctionBase)
                sExecuteCode &= oFunc.GetOpenGlAddCode(oChild.ToPin, sTempVarName)
            End If
        Next

        Return sExecuteCode
    End Function



End Class
