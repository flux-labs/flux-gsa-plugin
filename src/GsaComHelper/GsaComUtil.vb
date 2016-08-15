'
' © 2008 Oasys Ltd.
'
Imports System.IO
Imports System.Math
Imports System.Reflection
Imports System.Collections.Generic


Public Class GsaComUtil

    Const RoundPrecision As Integer = 6 'precision for rounding real numbers
    Const SectionSid_Usage As String = "usage"
    Const SectionSid_Symbol As String = "symbol"

    Enum EntType
        SEL_ELEM = 2
        SEL_MEMBER = 3
    End Enum
    Enum Mat
        STEEL = 0
        CONC_SHORT = -1
        CONC_LONG = -2
        ALUMINIUM = -3
        GLASS = -4
        NO_MAT = -5
    End Enum
  
    Enum ElemType
        EL_UNDEF = 0
        EL_BEAM = 201
        EL_BAR = 202
        EL_TIE = 203
        EL_STRUT = 204
        EL_SPRING = 205
        EL_LINK = 206
        EL_CABLE = 207
        EL_SPACER = 208
        EL_GROUND = 101
        EL_MASS = 102
        EL_QUAD4 = 401
        EL_QUAD8 = 801
        EL_TRI3 = 301
        EL_TRI6 = 601
        EL_PLANESTRESS = 901
        EL_FLATPLATE = 902
    End Enum
    Public Enum MembType

        MB_UNDEF = -1
        MB_UNDEF_ARC = -2
        MB_UNDEF_RAD = -3
        MB_UNDEF_EXP = -4

        MB_BEAM = 1
        MB_BEAM_ARC = 2
        MB_BEAM_RAD = 3
        MB_BEAM_EXP = 4

        MB_COL = 5
        MB_COL_ARC = 6
        MB_COL_RAD = 7
        MB_COL_EXP = 8

    End Enum
    Public Enum MembMat
        MT_UNDEF = -1
        MT_STEEL = 1
        MT_CONCRETE = 2
    End Enum
    Public Enum SectionMatch_Flags
        NONE = 0
        SEC_INCL_SS = 1
        SEC_ATTEMPT_STD = 2
        BOTH = (SectionMatch_Flags.SEC_INCL_SS Or SectionMatch_Flags.SEC_ATTEMPT_STD)
    End Enum

    Public Enum SectionUsage
        NOT_USED = 0
        FRAMING = GsRevit_Usage.FRAMING
        COLUMNS = GsRevit_Usage.COLUMNS
        INVALID = FRAMING Or COLUMNS
    End Enum
    Public Enum Units
        IMPERIAL = GsRevit_Units.IMPERIAL
        METRIC = GsRevit_Units.METRIC
    End Enum

    'GSA object
    Private m_GSAObject As ComAuto
    Private m_eSelType As EntType
    Private m_eUnit As GsRevit_Units ' Units of the REVIT MODEL!! Careful
    Public m_cfLength As Double = 1
    Public m_cfactor As Double = 1

    Public Sub New()
        Try
            m_GSAObject = New ComAuto()
        Catch ex As System.Runtime.InteropServices.COMException
            Throw New System.Exception("Cannot initialise GSA object. " & ex.Message)
        End Try
        If m_GSAObject Is Nothing Then
            Throw New System.Exception("Cannot initialise GSA object")
        End If
        ' we parse using the EN_GB locale
        m_GSAObject.SetLocale(Locale.LOC_EN_GB)
    End Sub

    'release GSA object
    Public Sub ReleaseGsa()
        m_GSAObject = Nothing
    End Sub

    Public Sub GsaUpdateViews()
        Me.m_GSAObject.UpdateViews()
    End Sub

    'open a new GSA file
    Public Function GsaNewFile() As Short
        Return Me.m_GSAObject.NewFile()
    End Function

    'open an existing GSA file
    Public Function GsaOpenFile(ByRef sFileName As String) As Short
        Return Me.m_GSAObject.Open(sFileName)
    End Function

    'save the current GSA model to file
    Public Function GsaSaveFile(Optional ByVal sFileName As String = "") As Short
        If Not String.IsNullOrEmpty(sFileName) Then
            Return Me.m_GSAObject.SaveAs(sFileName)
        Else
            Return Me.m_GSAObject.Save()
        End If
    End Function
    Public Function GsaObj() As ComAuto
        Return m_GSAObject
    End Function
    'close GSA model
    Public Function GsaCloseFile() As Short
        Dim returnVal As Short = m_GSAObject.Close()
        Me.ReleaseGsa()
        Return returnVal
    End Function

    'delete GSA results
    Public Function GsaDeleteResults() As Short
        Return Me.m_GSAObject.Delete("RESULTS")
    End Function

    'delete the GSA results and cases
    Public Function GsaDeleteResultsAndCases() As Short
        Return Me.m_GSAObject.Delete("RESULTS_AND_CASES")
    End Function

    'analyse the GSA model
    Public Sub GsaAnalyse()
        Me.m_GSAObject.Analyse()
    End Sub
    Public Sub LogFeatureUsage(ByVal LogName As String)
        Me.m_GSAObject.LogFeatureUsage(LogName)
    End Sub

    Public ReadOnly Property GsaComObject() As ComAuto
        Get
            Return m_GSAObject
        End Get
    End Property
    Public Property RevitUnits() As GsaComUtil.Units
        Get
            Return CType(m_eUnit, GsaComUtil.Units)
        End Get
        Set(ByVal value As GsaComUtil.Units)
            m_eUnit = CType(value, GsRevit_Units)
        End Set
    End Property
    Public Function MappingPath() As String
        Dim cPath As String = ""
        m_GSAObject.MappingDBPath(cPath)
        Return cPath
    End Function
    Public Function RevitFamilyToSection(ByVal familyName As String, ByVal familyType As String, ByVal usage As SectionUsage) As String
        Dim gsrevit_usage As GsRevit_Usage = gsa_8_7.GsRevit_Usage.FRAMING
        If SectionUsage.COLUMNS = usage Then
            gsrevit_usage = gsa_8_7.GsRevit_Usage.COLUMNS
        End If
        Return m_GSAObject.Gen_SectTransltnGsRevit(familyName, GsRevit_SectTrnsDir.REVIT_TO_GSA, gsrevit_usage, familyType)
    End Function
    Public Function SectionToRevitFamily(ByVal gsaDesc As String, _
                                         ByVal usage As SectionUsage, _
                                         ByRef familyName As String, _
                                         ByRef bFamilyTypeFound As Boolean) As String
        Dim familyType As String = ""
        Dim gsrevit_usage As GsRevit_Usage = gsa_8_7.GsRevit_Usage.FRAMING
        If SectionUsage.COLUMNS = usage Then
            gsrevit_usage = gsa_8_7.GsRevit_Usage.COLUMNS
        End If
        familyName = ""
        familyType = m_GSAObject.Gen_SectTransltnGsRevit(gsaDesc, GsRevit_SectTrnsDir.GSA_TO_REVIT, gsrevit_usage, familyName)
        If Not String.IsNullOrEmpty(familyType) And Not String.IsNullOrEmpty(familyName) Then
            bFamilyTypeFound = True
        Else
            familyType = TrySNFamilies(gsaDesc, usage, bFamilyTypeFound, familyName)
        End If
        Return familyType
    End Function
    Public Function SectionToRevitFamily(ByVal gsaDesc As String, _
                                        ByVal usage As SectionUsage, _
                                        ByRef familyName As String, _
                                        ByRef bFamilyTypeFound As Boolean, ByRef bFamilyFromTrial As Boolean) As String
        Dim familyType As String = ""
        Dim gsrevit_usage As GsRevit_Usage = gsa_8_7.GsRevit_Usage.FRAMING
        If SectionUsage.COLUMNS = usage Then
            gsrevit_usage = gsa_8_7.GsRevit_Usage.COLUMNS
        End If
        familyName = ""
        familyType = m_GSAObject.Gen_SectTransltnGsRevit(gsaDesc, GsRevit_SectTrnsDir.GSA_TO_REVIT, gsrevit_usage, familyName)
        If Not String.IsNullOrEmpty(familyType) And Not String.IsNullOrEmpty(familyName) Then
            bFamilyTypeFound = True
            bFamilyFromTrial = False
        Else
            familyType = TrySNFamilies(gsaDesc, usage, bFamilyTypeFound, familyName)
            bFamilyFromTrial = True
        End If

        'Dim arrp1() As gsa_8_6.Results = Nothing
        'Dim arr1() As Double = arrp1.dynaResults

        Return familyType
    End Function
    Private Function TrySNFamilies(ByVal desc As String, ByVal usage As GsaComUtil.SectionUsage, ByRef familyTypeFound As Boolean, ByRef familyName As String) As String
        Dim parts As String() = Nothing
        parts = desc.Split(New [Char]() {"%"c, " "c}, StringSplitOptions.RemoveEmptyEntries)
        Dim typeName As String = ""
        If String.Equals(parts.GetValue(0), "CAT") Then
            'CAT W W14x43
            If Me.CATSectionToSNFamily(parts, usage, familyName) Then
                familyTypeFound = True
                Return parts(2)
            End If
        ElseIf String.Equals(parts.GetValue(0), "STD") Then
            If Me.STDSectionToSNFamily(parts, usage, familyName) Then
                familyTypeFound = False
                Return Nothing
            End If
        End If
        'Debug.Assert(False)
        familyName = ""
        familyTypeFound = False
        Return Nothing

    End Function
    ''' <summary>
    ''' calls Gen_MatchDesc. Options include bSuperSeded, bAttemptStd
    ''' </summary>
    ''' <param name="sDesc"></param>
    ''' <param name="options"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MatchDescription(ByVal sDesc As String, ByVal options As GsaComUtil.SectionMatch_Flags) As String
        If String.IsNullOrEmpty(sDesc) Then
            Return String.Empty
        End If
        Dim result As String = ""
        Try
            result = m_GSAObject.Gen_SectionMatchDesc(sDesc, options)
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
        Return result
    End Function

    'find existing GSA node at required position or create a new one
    Public Function NodeAt(ByVal dX As Double, ByVal dY As Double, ByVal dZ As Double, _
                                ByVal dCoincidenceTol As Double) As Integer
        Dim iNode As Integer = 0

        'round
        dX = Math.Round(dX, RoundPrecision)
        dY = Math.Round(dY, RoundPrecision)
        dZ = Math.Round(dZ, RoundPrecision)

        'find existing node within tolerance or create new node
        iNode = Me.m_GSAObject.Gen_NodeAt(dX, dY, dZ, dCoincidenceTol)
        Return iNode
    End Function
    Public Sub SetGridNode(ByVal iNode As Integer, ByVal iGrid As Integer)
        'NODE_GRID | ref | name | grid plane | grid | grid line a | grid line b | edge length | radius | column rigidity
        'NODE_GRID	77	a	0	ORIGIN			0.000000	0.000000	NO

        'NODE.2 | num | name | colour | x | y | z |
        '   is_grid { | grid_plane | datum | grid_line_a | grid_line_b } | axis |
        '   is_rest { | rx | ry | rz | rxx | ryy | rzz } |
        '   is_stiff { | Kx | Ky | Kz | Kxx | Kyy | Kzz } |
        '   is_mesh { | edge_length | radius | column_rigidity | column_prop | column_node | column_angle | column_factor | column_slab_factor }

        'Dim sGwaCommand As String = "NODE_GRID,"
        'sGwaCommand += iNode.ToString() + ","
        'sGwaCommand += "" + ","                     'Name
        'sGwaCommand += ",NO_RGB"                    'colour
        'sGwaCommand += iGrid.ToString() + ","       'Global grid
        'sGwaCommand += "ORIGIN,,,"                  'Grid lines
        'sGwaCommand += "0.0,0.0,NO"
        'm_GSAObject.GwaCommand(sGwaCommand)

    End Sub
    Public Function BlankElement(ByVal iELem As Integer) As Boolean
        Dim gwaCommand As String
        gwaCommand = "BLANK,EL," + iELem.ToString()
        Dim check As Object = m_GSAObject.GwaCommand(gwaCommand)
        Return True
    End Function
    Public Function BlankMember(ByVal iMem As Integer) As Boolean
        Dim gwaCommand As String
        gwaCommand = "BLANK,MEMB," + iMem.ToString()
        Dim check As Object = m_GSAObject.GwaCommand(gwaCommand)
        Return True
    End Function
    Public Sub ChangeUnits(ByRef dData() As Double)
        dData(0) = Math.Round(dData(0) * m_cfLength, 4)
        dData(1) = Math.Round(dData(1) * m_cfLength, 4)
        dData(2) = Math.Round(dData(2) * m_cfLength, 4)
    End Sub
    Public Sub ExtractNodeCoor(ByVal strNode As String, ByRef x As Double, ByRef y As Double, ByRef z As Double)

        Dim iNode As Integer
        'initialize the coords to 0 first
        x = 0
        y = 0
        z = 0
        If Not (Integer.TryParse(strNode, iNode)) Then
            Exit Sub
        End If
        Dim check As Object
        Dim gwaCommand As String
        gwaCommand = "EXIST,NODE," + strNode
        check = m_GSAObject.GwaCommand(gwaCommand)
        Dim iCheck As Integer = CType(check, Integer)
        If 1 = iCheck Then
            m_GSAObject.NodeCoor(iNode, x, y, z)
        End If
        'change SI unit to user unit
        x = x * m_cfactor
        y = y * m_cfactor
        z = z * m_cfactor

    End Sub
    Public Function ExtractInterMediateNodeCoorOnCurve(ByVal strMembRef As String) As Double()

        Dim iMemb As Integer
        'initialize the coords to 0 first
        Dim dbNoderCord() As Double = {0.0, 0.0, 0.0}
        If Not (Integer.TryParse(strMembRef, iMemb)) Then
            Return dbNoderCord
        End If
        Dim check As String = ""
        check = CStr(m_GSAObject.GwaCommand("GET,MEMB," & strMembRef))
        If Not String.IsNullOrEmpty(check) Then
            m_GSAObject.MembCoorOnCurve(iMemb, dbNoderCord(0), dbNoderCord(1), dbNoderCord(2))
        End If

        'change SI unit to user unit
        dbNoderCord(0) = dbNoderCord(0) * m_cfactor
        dbNoderCord(1) = dbNoderCord(1) * m_cfactor
        dbNoderCord(2) = dbNoderCord(2) * m_cfactor
        Return dbNoderCord

    End Function
    Public Function ExtractNodeCoor(ByVal strNode As String) As Double()

        Dim iNode As Integer
        'initialize the coords to 0 first
        Dim dbNoderCord() As Double = {0.0, 0.0, 0.0}
        If Not (Integer.TryParse(strNode, iNode)) Then
            Return dbNoderCord
        End If
        Dim check As Object
        Dim gwaCommand As String
        gwaCommand = "EXIST,NODE," + strNode
        check = m_GSAObject.GwaCommand(gwaCommand)
        Dim iCheck As Integer = CType(check, Integer)
        If 1 = iCheck Then
            m_GSAObject.NodeCoor(iNode, dbNoderCord(0), dbNoderCord(1), dbNoderCord(2))
        End If
        'change SI unit to user unit
        dbNoderCord(0) = dbNoderCord(0) * m_cfactor
        dbNoderCord(1) = dbNoderCord(1) * m_cfactor
        dbNoderCord(2) = dbNoderCord(2) * m_cfactor
        Return dbNoderCord


    End Function
    Public Shared Function Arg(ByVal pos As Integer, ByVal source As String) As String
        Dim strArray As String() = source.Split(New [Char]() {","c})
        If strArray.Length > pos Then
            Return CType(strArray.GetValue(pos), String)
        Else
            Return String.Empty
        End If
    End Function
    Public Sub AssignUnitsCF()
        ' Assign units conversion factors
        'If m_revitObj.ActiveDocument.DisplayUnitSystem = Autodesk.Revit.Enums.DisplayUnit.IMPERIAL Then
        '    m_cfLength = 3.28 ' m to ft
        'End If
        Dim commandObj As Object = m_GSAObject.GwaCommand("GET,UNIT_DATA, LENGTH")
        If commandObj Is Nothing Then
            Exit Sub
        End If
        Dim commandResult As String = commandObj.ToString()
        'UNIT_DATA | option | name | factor
        'UNIT_DATA,LENGTH,ft,3.28084

        Dim factor As Double = 1
        Double.TryParse(GsaComUtil.Arg(3, commandResult), factor)
        m_cfLength = 3.28084 / factor
        m_cfactor = factor
        ' A factor which can convert the values to feet - THis is what the revit API expects.
    End Sub
    'write a GSA Grid Plane
    Public Function SetGridPlane(ByVal sid As String, ByVal iGrid As Integer, ByVal sName As String, ByVal iAxis As Integer, ByVal dElev As Double, ByVal sList As String, ByVal sSpan As String, ByVal dAngle As Double, ByVal iLayout As Integer, ByVal dTol As Double) As Integer
        Dim sGwaCommand As String = ""

        If (0 = iGrid) Then
            iGrid = Me.HighestGridPlane() + 1
        End If

        'round
        dElev = Math.Round(dElev, RoundPrecision)

        'write grid plane
        sGwaCommand = "GRID_PLANE:"
        sGwaCommand += sid
        sGwaCommand += "," + iGrid.ToString()       'number
        sGwaCommand += "," & sName                  'name
        sGwaCommand += "," + iAxis.ToString()       'axis
        sGwaCommand += "," + "ELEV"                 'option
        sGwaCommand += "," + dElev.ToString()       'elevation [feet]
        sGwaCommand += "," + sList                  'elements
        sGwaCommand += "," + sSpan                  'span
        sGwaCommand += "," + dAngle.ToString()      'angle
        sGwaCommand += "," + iLayout.ToString()     'layout
        sGwaCommand += "," + dTol.ToString()        'plane_tol [feet]
        m_GSAObject.GwaCommand(sGwaCommand)

        Return iGrid
    End Function

    Public Function GridPlane(ByVal iNum As Integer, ByRef sName As String, ByRef uid As String, ByRef dElev As Double) As Boolean
        Dim bResult As Boolean = False
        If Me.GridPlane(iNum, sName, dElev) Then
            bResult = True
        Else
            Return False
        End If
        uid = m_GSAObject.GetSidTagValue("GRID_PLANE", iNum, "RVT")
        Return bResult
    End Function
    Public Function GridPlane(ByVal iNum As Integer, ByRef sName As String, ByRef dElev As Double) As Boolean

        Dim iAxis As Integer = 0
        Dim list As String = ""
        Dim span As String = ""
        Dim dAngle As Double = 0
        Dim iLayout As Integer = 0
        Dim dTol As Double = 0
        Return Me.GridPlane(iNum, sName, iAxis, dElev, list, span, dAngle, iLayout, dTol)

    End Function
    'read a GSA Grid Plane
    Public Function GridPlane(ByVal iGrid As Integer, ByRef sName As String, ByRef iAxis As Integer, ByRef dElev As Double, ByRef sList As String, ByRef sSpan As String, ByRef dAngle As Double, ByRef iLayout As Integer, ByRef dTol As Double) As Boolean
        If Not Me.GridPlaneExists(iGrid) Then
            Return False
        End If

        Dim sGwaCommand As String = ""
        Dim sArg As String

        sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,GRID_PLANE," & iGrid.ToString))
        If String.IsNullOrEmpty(sGwaCommand) Then
            Return False
        End If
        sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
        sArg = GsaComUtil.Arg(2, sGwaCommand) 'name
        sName = sArg
        sArg = GsaComUtil.Arg(3, sGwaCommand) 'axis
        iAxis = CInt(sArg)
        'sArg = GsaObj.Arg(4, sGwaCommand) 'option (not used)
        'iOption = Val(sArg)
        sArg = GsaComUtil.Arg(5, sGwaCommand) 'elevation [feet]
        dElev = Val(sArg)
        sArg = GsaComUtil.Arg(6, sGwaCommand) 'elements
        sList = sArg
        sArg = GsaComUtil.Arg(7, sGwaCommand) 'span
        sSpan = sArg
        sArg = GsaComUtil.Arg(8, sGwaCommand) 'angle
        dAngle = Val(sArg)
        sArg = GsaComUtil.Arg(9, sGwaCommand) 'layout
        iLayout = CInt(sArg)
        sArg = GsaComUtil.Arg(10, sGwaCommand) 'plane_tol [feet]
        dTol = Val(sArg)
        Return True
    End Function

    'write GSA grid line
    Public Function SetGridLine(ByVal iGrLine As Integer, ByVal sName As String, ByVal bArc As Boolean, ByVal sid As String, _
                                   ByVal coorX As Double, ByVal coorY As Double, ByVal length As Double, _
                                   Optional ByVal theta1 As Double = 0.0, Optional ByVal theta2 As Double = 0.0) As Boolean

        Dim sGwaCommand As String = ""
        'GRID_LINE | num | name | arc | coor_x | coor_y | length | theta1 | theta2

        sGwaCommand = "GRID_LINE,"
        sGwaCommand += iGrLine.ToString() + ","
        sGwaCommand += sName + ","
        If bArc Then
            sGwaCommand += "ARC"
        End If
        sGwaCommand += ","
        sGwaCommand += coorX.ToString() + ","
        sGwaCommand += coorY.ToString() + ","
        sGwaCommand += length.ToString() + ","
        sGwaCommand += theta1.ToString() + "," + theta2.ToString() + ","

        m_GSAObject.GwaCommand(sGwaCommand)
        Dim iGrLineNew As Integer = Me.HighestGridLine()

        If Int32.Equals(iGrLine, iGrLineNew) Then
            m_GSAObject.WriteSidTagValue("GRID_LINE", iGrLine, "RVT", sid)
            Return True
        Else
            Return False
        End If

    End Function
    Public Function GridLine(ByVal iGrLine As Integer, ByRef name As String, ByRef bArc As Boolean, ByRef sid As String, _
                                    ByRef coorX As Double, ByRef coorY As Double, ByRef len As Double, _
                                    ByRef theta1 As Double, ByRef theta2 As Double) As Boolean

        'GRID_LINE | num | name | arc | coor_x | coor_y | length | theta1 | theta2

        If Not Me.GridLineExists(iGrLine) Then
            Return False
        End If

        Dim result As String = CStr(m_GSAObject.GwaCommand("GET,GRID_LINE," & iGrLine.ToString()))
        Dim arg As String = GsaComUtil.Arg(1, result)
        Dim iLine As Integer = CInt(arg)
        If Not Int32.Equals(iLine, iGrLine) Then
            Return False
        End If

        arg = GsaComUtil.Arg(2, result)
        name = arg

        arg = GsaComUtil.Arg(3, result)
        If String.IsNullOrEmpty(arg) Then
            bArc = False
        Else
            bArc = True
        End If

        arg = GsaComUtil.Arg(4, result)
        Double.TryParse(arg, coorX)

        arg = GsaComUtil.Arg(5, result)
        Double.TryParse(arg, coorY)

        arg = GsaComUtil.Arg(6, result)
        Double.TryParse(arg, len)

        arg = GsaComUtil.Arg(7, result)
        Double.TryParse(arg, theta1)

        arg = GsaComUtil.Arg(8, result)
        Double.TryParse(arg, theta2)

        sid = m_GSAObject.GetSidTagValue("GRID_LINE", iGrLine, "RVT")

        Return True
    End Function
    'write a GSA 1D element
    Public Function SetElem1d(ByVal iElem As Integer, ByVal iProp As Integer, ByVal sID As String,
                               ByVal iTopoList As List(Of Integer), ByVal iOrNode As Integer, ByVal dBeta As Double,
                               ByVal sRelease0 As String, ByVal sRelease1 As String,
                               ByRef dOffset0() As Double, ByRef dOffset1() As Double, ByRef dummy As String
                               ) As Integer

        Dim sGwaCommand As String = ""

        If (0 = iElem) Then
            iElem = Me.HighestEnt("EL") + 1
        End If

        'round
        dBeta = Math.Round(dBeta, RoundPrecision)

        'write beam element
        'EL_BEAM | num | prop | group | topo(2) | node | angle | dummy
        sGwaCommand = "EL_BEAM:"
        sGwaCommand += "{RVT:" & sID & "}"
        sGwaCommand += "," + iElem.ToString()       'number
        sGwaCommand += "," + iProp.ToString()       'property
        sGwaCommand += ",1"                         'group
        sGwaCommand += "," + iTopoList.Item(0).ToString()      'topo 0
        sGwaCommand += "," + iTopoList.Item(1).ToString()      'topo 1
        sGwaCommand += "," + iOrNode.ToString()     'orientation node
        sGwaCommand += "," + dBeta.ToString()       'orientation angle
        If (Not String.IsNullOrEmpty(dummy)) Then
            sGwaCommand += "," + dummy              'dummy
        End If
        m_GSAObject.GwaCommand(sGwaCommand)

        'write releases
        'EL_RLS | num | rls()
        sGwaCommand = "EL_RLS"
        sGwaCommand += "," + iElem.ToString()       'number
        sGwaCommand += "," + sRelease0              'release 0
        sGwaCommand += "," + sRelease1              'release 1
        m_GSAObject.GwaCommand(sGwaCommand)

        'write offsets
        'EL_OFFSET | num | item | Ox | Oy | Oz
        sGwaCommand = "EL_OFFSET"
        sGwaCommand += "," + iElem.ToString()       'number
        sGwaCommand += ",0"                         'position
        sGwaCommand += "," + dOffset0(0).ToString() 'X
        sGwaCommand += "," + dOffset0(1).ToString() 'Y
        sGwaCommand += "," + dOffset0(2).ToString() 'Z
        m_GSAObject.GwaCommand(sGwaCommand)

        sGwaCommand = "EL_OFFSET"
        sGwaCommand += "," + iElem.ToString()       'number
        sGwaCommand += ",1"                         'position
        sGwaCommand += "," + dOffset1(0).ToString() 'X
        sGwaCommand += "," + dOffset1(1).ToString() 'Y
        sGwaCommand += "," + dOffset1(2).ToString() 'Z
        m_GSAObject.GwaCommand(sGwaCommand)

        Return iElem
    End Function
    'write a GSA 1D element
    Public Function SetElem1d(ByVal iElem As Integer, ByVal iProp As Integer, ByVal sID As String,
                               ByVal iTopo0 As Integer, ByVal iTopo1 As Integer, ByVal iOrNode As Integer, ByVal dBeta As Double,
                               ByVal sRelease0 As String, ByVal sRelease1 As String,
                               ByRef dOffset0() As Double, ByRef dOffset1() As Double, dummy As String) As Integer

        Dim sGwaCommand As String = ""

        If (0 = iElem) Then
            iElem = Me.HighestEnt("EL") + 1
        End If

        'round
        dBeta = Math.Round(dBeta, RoundPrecision)

        'write beam element
        'EL_BEAM | num | prop | group | topo(2) | node | angle | dummy
        sGwaCommand = "EL_BEAM:"
        sGwaCommand += "{RVT:" & sID & "}"
        sGwaCommand += "," + iElem.ToString()       'number
        sGwaCommand += "," + iProp.ToString()       'property
        sGwaCommand += ",1"                         'group
        sGwaCommand += "," + iTopo0.ToString()      'topo 0
        sGwaCommand += "," + iTopo1.ToString()      'topo 1
        sGwaCommand += "," + iOrNode.ToString()     'orientation node
        sGwaCommand += "," + dBeta.ToString()       'orientation angle
        If (Not String.IsNullOrEmpty(dummy)) Then
            sGwaCommand += "," + dummy              'dummy
        End If

        Dim result As Object
        result = m_GSAObject.GwaCommand(sGwaCommand)

        'write releases
        'EL_RLS | num | rls()
        sGwaCommand = "EL_RLS"
        sGwaCommand += "," + iElem.ToString()       'number
        sGwaCommand += "," + sRelease0              'release 0
        sGwaCommand += "," + sRelease1              'release 1
        m_GSAObject.GwaCommand(sGwaCommand)

        'write offsets
        'EL_OFFSET | num | item | Ox | Oy | Oz
        sGwaCommand = "EL_OFFSET"
        sGwaCommand += "," + iElem.ToString()       'number
        sGwaCommand += ",0"                         'position
        sGwaCommand += "," + dOffset0(0).ToString() 'X
        sGwaCommand += "," + dOffset0(1).ToString() 'Y
        sGwaCommand += "," + dOffset0(2).ToString() 'Z
        m_GSAObject.GwaCommand(sGwaCommand)

        sGwaCommand = "EL_OFFSET"
        sGwaCommand += "," + iElem.ToString()       'number
        sGwaCommand += ",1"                         'position
        sGwaCommand += "," + dOffset1(0).ToString() 'X
        sGwaCommand += "," + dOffset1(1).ToString() 'Y
        sGwaCommand += "," + dOffset1(2).ToString() 'Z
        m_GSAObject.GwaCommand(sGwaCommand)

        Return iElem
    End Function

    Public Function Node(ByVal iNode As Integer, ByRef name As String, ByRef x As Double,
                       ByRef y As Double, ByRef z As Double) As Boolean

        'NODE | num | name | colour | x | y | z
        'NODE,1,,NO_RGB,745057.3125,528235.6875,518849.6250

        If Not Me.EntExists("NODE", iNode) Then
            Return False
        End If
        'Dim eType As ElemType
        Dim sGwaCommand As String = ""
        sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,NODE," & iNode.ToString))

        If String.IsNullOrEmpty(sGwaCommand) Then
            Return False
        End If

        Dim sArg As String

        sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
        Debug.Assert(Integer.Equals(iNode, CInt(sArg)))

        sArg = GsaComUtil.Arg(2, sGwaCommand) 'name
        name = sArg

        sArg = GsaComUtil.Arg(4, sGwaCommand) 'x
        Double.TryParse(sArg, x)

        sArg = GsaComUtil.Arg(5, sGwaCommand) 'y
        Double.TryParse(sArg, y)

        sArg = GsaComUtil.Arg(6, sGwaCommand) 'z
        Double.TryParse(sArg, z)
    End Function

    'read a GSA 1D element
    Public Function Elem1d(ByVal iElem As Integer, ByRef iProp As Integer, ByRef uid As String, _
            ByRef iTopoList As List(Of Integer), ByRef iOrNode As Integer, ByRef dBeta As Double, _
            ByRef sRelease0 As String, ByRef sRelease1 As String, _
            ByRef dOffset0() As Double, ByRef dOffset1() As Double, ByRef strDummy As String) As Boolean

        'EL | num | name | colour | type | prop | group | topo() | node | angle |
        'is_rls { | rls() } | is_offset { | ox | oy | oz } | dummy 
        'EL,1,,NO_RGB,BEAM,1,1,1,2,0,0.000000,RLS,FPF,FFF

        If Not Me.EntExists("EL", iElem) Then
            Return False
        End If
        Dim eType As ElemType
        Dim sGwaCommand As String = ""
        sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,EL," & iElem.ToString))

        If String.IsNullOrEmpty(sGwaCommand) Then
            Return False
        End If

        Dim sArg As String
        uid = ""
        sArg = GsaComUtil.Arg(0, sGwaCommand)
        Dim idString As String = GsaComUtil.ExtractId(sArg)
        If Not String.IsNullOrEmpty(idString) Then
            uid = idString
        End If

        sArg = GsaComUtil.Arg(4, sGwaCommand)
        eType = Me.ElemTypeFromString(sArg)

        If Not GsaComUtil.ElemTypeIsBeamOrTruss(eType) Then
            Return False
        End If

        sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
        Debug.Assert(Integer.Equals(iElem, CInt(sArg)))
        sArg = GsaComUtil.Arg(5, sGwaCommand) 'property
        iProp = CInt(sArg)

        sArg = GsaComUtil.Arg(7, sGwaCommand) 'topo 0
        iTopoList.Add(CInt(sArg))

        sArg = GsaComUtil.Arg(8, sGwaCommand) 'topo 1
        iTopoList.Add(CInt(sArg))

        sArg = GsaComUtil.Arg(9, sGwaCommand) 'orientation node
        Integer.TryParse(sArg, iOrNode)
        sArg = GsaComUtil.Arg(10, sGwaCommand) 'orientation angle
        Double.TryParse(sArg, dBeta)

        Dim position As Integer = 11
        'releases
        sArg = GsaComUtil.Arg(position, sGwaCommand)
        If String.Equals("RLS", sArg) Then
            position += 1
            sRelease0 = GsaComUtil.Arg(position, sGwaCommand) 'release 0
            position += 1
            sRelease1 = GsaComUtil.Arg(position, sGwaCommand) 'release 1
        Else
            sRelease0 = "FFF"
            sRelease1 = "FFF"
        End If

        'offsets
        position += 1
        sArg = GsaComUtil.Arg(position, sGwaCommand)
        If String.Equals("OFFSET", sArg) Then
            position += 1
            sArg = GsaComUtil.Arg(position, sGwaCommand)    'offset 0, X
            dOffset0(0) = Val(sArg)

            position += 1
            sArg = GsaComUtil.Arg(position, sGwaCommand)    'offset 0, Y
            dOffset0(1) = Val(sArg)

            position += 1
            sArg = GsaComUtil.Arg(position, sGwaCommand)    'offset 0, Z
            dOffset0(2) = Val(sArg)

            position += 1
            sArg = GsaComUtil.Arg(position, sGwaCommand)
            dOffset1(0) = Val(sArg)

            position += 1
            sArg = GsaComUtil.Arg(position, sGwaCommand)
            dOffset1(1) = Val(sArg)

            position += 1
            sArg = GsaComUtil.Arg(position, sGwaCommand)
            dOffset1(2) = Val(sArg)

        Else
            dOffset0(0) = 0.0
            dOffset0(1) = 0.0
            dOffset0(2) = 0.0
            dOffset1(0) = 0.0
            dOffset1(1) = 0.0
            dOffset1(2) = 0.0
        End If
        position += 1
        strDummy = GsaComUtil.Arg(position, sGwaCommand)

        Return True
    End Function
    Public Function NumElementInMember(ByVal i As Integer) As Integer
        Return m_GSAObject.MembNumElem(i)
    End Function
    Public Function ElementNumber(ByVal iMemb As Integer, ByVal iNdex As Integer) As Integer
        Return m_GSAObject.MembElemNum(iMemb, iNdex)
    End Function


    'write a GSA 1D element
    Public Function SetMember(ByVal iElem As Integer, ByVal sName As String, ByVal iProp As Integer, _
                                ByVal sID As String, ByRef iTopoList As List(Of Integer), ByVal dRadius As Double, _
                                ByVal iOrNode As Integer, ByVal dBeta As Double, _
                                ByVal sRelease0 As String, ByVal sRelease1 As String, _
                                ByRef dOffset0() As Double, ByRef dOffset1() As Double, _
                                ByVal type As MembType, ByVal mat As MembMat) As Integer

        If (0 = iElem) Then
            iElem = Me.HighestEnt("MEMBER") + 1
        End If
        '   MEMB.2 | num | name | colour | MT_STEEL | type | section | design | 
        '       restraint | group | topo(2) | node | angle | rls(2) | { Ox | Ox | Oz } |
        '       is_curved { | geom | topo3 | radius | facet_type | facet_value  } @end

        '   MEMB.2 | num | name | colour | MT_CONCRETE | type | section | design | 
        '       num_arrange | { arrange } | group | topo(2) | node | angle | rls(2) | { Ox | Ox | Oz } |
        '       is_curved { | geom | topo3 | radius | facet_type | facet_value  } @end
        '
        '   MEMB.2 | num | name | colour | MT_UNDEF | type | section | group | 
        '       topo(2) | node | angle | rls(2) | { Ox | Ox | Oz } | 
        '       is_curved { | geom | topo3 | radius | facet_type | facet_value  } @end

        'round

        dBeta = Math.Round(dBeta, RoundPrecision)
        Dim sGwaCommand As String = SetMemberString(iElem, sName, iProp, 1, 1, sID, iTopoList, dRadius, iOrNode, dBeta, type, mat)
        'write beam element

        'write releases
        sGwaCommand += ",RLS"                       'Relese
        sGwaCommand += "," + sRelease0              'release 0
        sGwaCommand += "," + sRelease1              'release 1

        'write offsets
        sGwaCommand += ",OFFSET"
        sGwaCommand += "," + dOffset0(0).ToString() 'X
        sGwaCommand += "," + dOffset0(1).ToString() 'Y
        sGwaCommand += "," + dOffset0(2).ToString() 'Z

        sGwaCommand += "," + dOffset1(0).ToString() 'X
        sGwaCommand += "," + dOffset1(1).ToString() 'Y
        sGwaCommand += "," + dOffset1(2).ToString() 'Z
        m_GSAObject.GwaCommand(sGwaCommand)
        Return iElem
    End Function

    Private Function SetMemberString(ByVal iElem As Integer, ByRef sName As String, ByVal iProp As Integer, ByVal iDesign As Integer, _
                                     ByVal iRest As Integer, ByVal sID As String, _
                                    ByRef iTopoList As List(Of Integer), ByVal dRadius As Double, _
                                     ByVal iOrNode As Integer, ByVal dBeta As Double, _
                                     ByVal type As MembType, ByVal mat As MembMat) As String
        Dim sGwaCommand As String = ""
        sGwaCommand = "MEMB:"
        sGwaCommand += "{RVT:" & sID & "}"
        sGwaCommand += "," + iElem.ToString()       'number
        sGwaCommand += "," + sName                          'name
        sGwaCommand += ",NO_RGB"                    'colour
        sGwaCommand += "," + mat.ToString()         ' member material
        sGwaCommand += "," + type.ToString()        ' member type
        sGwaCommand += "," + iProp.ToString()       'section

        Select Case mat
            Case MembMat.MT_STEEL
                sGwaCommand += "," + iDesign.ToString()             'design
                sGwaCommand += "," + iRest.ToString()               'restraint
                sGwaCommand += ",1"                                 'group
                sGwaCommand += "," + iTopoList.Item(0).ToString()   'topo 0
                sGwaCommand += "," + iTopoList.Item(1).ToString()   'topo 1
                For iExp As Integer = 2 To iTopoList.Count - 1
                    sGwaCommand += "," + iTopoList.Item(iExp).ToString()
                Next iExp
                sGwaCommand += "," + dRadius.ToString()     'radius
                sGwaCommand += "," + iOrNode.ToString()     'orientation node
                sGwaCommand += "," + dBeta.ToString()       'orientation angle
            Case MembMat.MT_CONCRETE
                sGwaCommand += "," + iDesign.ToString()     'design
                sGwaCommand += ",1"                         'num_arrangements    
                sGwaCommand += ", "                         '{arrange}
                sGwaCommand += ",1"                         ' group
                sGwaCommand += "," + iTopoList.Item(0).ToString()   'topo 0
                sGwaCommand += "," + iTopoList.Item(1).ToString()   'topo 1
                For iExp As Integer = 2 To iTopoList.Count - 1
                    sGwaCommand += "," + iTopoList.Item(iExp).ToString()
                Next iExp
                sGwaCommand += "," + dRadius.ToString()     'radius
                sGwaCommand += "," + iOrNode.ToString()     'orientation node
                sGwaCommand += "," + dBeta.ToString()       'orientation angle
            Case MembMat.MT_UNDEF
                sGwaCommand += ",1"                         ' group
                sGwaCommand += "," + iTopoList.Item(0).ToString()   'topo 0
                sGwaCommand += "," + iTopoList.Item(1).ToString()   'topo 1
                For iExp As Integer = 2 To iTopoList.Count - 1
                    sGwaCommand += "," + iTopoList.Item(iExp).ToString()
                Next iExp
                sGwaCommand += "," + dRadius.ToString()     'radius
                sGwaCommand += "," + iOrNode.ToString()     'orientation node
                sGwaCommand += "," + dBeta.ToString()       'orientation angle
        End Select
        Return sGwaCommand
    End Function
    Public Function GetMembType(ByVal iElem As Integer) As MembType
        Dim Type As MembType = MembType.MB_UNDEF
        Dim iTopoList As New List(Of Integer)
        If Not Me.EntExists("MEMBER", iElem) Then
            Exit Function
        End If
        Dim sGwaCommand As String = ""
        Dim sArg As String = ""
        sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,MEMB," & iElem.ToString))
        If String.IsNullOrEmpty(sGwaCommand) Then
            Exit Function
        End If
        Dim Mat As MembMat = MembMat.MT_UNDEF
        Dim num_parts As Integer = (sGwaCommand.Split(New Char() {","c})).Length
        sArg = GsaComUtil.Arg(4, sGwaCommand) 'membmat
        Select Case sArg
            Case "MT_STEEL"
                Mat = MembMat.MT_STEEL
            Case "MT_CONCRETE"
                Mat = MembMat.MT_CONCRETE
            Case "MT_UNDEF"
                Mat = MembMat.MT_UNDEF
        End Select
        Dim iExpNode As Integer = 0
        sArg = GsaComUtil.Arg(5, sGwaCommand)
        If sArg.Contains("MB_COLUMN_EXP") Then
            iExpNode = Convert.ToInt16(sArg.Remove(0, sArg.Length - 14))
            sArg = "MB_COLUMN_EXP"
        ElseIf sArg.Contains("MB_COL_EXP") Then
            iExpNode = Convert.ToInt16(sArg.Remove(0, sArg.Length - 11))
            sArg = "MB_COL_EXP"
        End If

        Select Case sArg
            Case "MB_BEAM"
                Type = MembType.MB_BEAM
            Case "MB_BEAM_ARC"
                Type = MembType.MB_BEAM_ARC
            Case "MB_BEAM_RAD"
                Type = MembType.MB_BEAM_RAD
            Case "MB_BEAM_EXP"
                Type = MembType.MB_BEAM_EXP

            Case "MB_COLUMN"
            Case "MB_COL"
                Type = MembType.MB_COL
            Case "MB_COLUMN_ARC"
            Case "MB_COL_ARC"
                Type = MembType.MB_COL
            Case "MB_COLUMN_RAD"
            Case "MB_COL_RAD"
                Type = MembType.MB_COL_ARC
            Case "MB_COLUMN_EXP"
            Case "MB_COL_EXP"
                Type = MembType.MB_COL_EXP

            Case "MB_UNDEF"
                Type = MembType.MB_UNDEF
            Case "MB_UNDEF_ARC"
                Type = MembType.MB_UNDEF_ARC
            Case "MB_UNDEF_RAD"
                Type = MembType.MB_UNDEF_RAD
            Case "MB_UNDEF_EXP"
                Type = MembType.MB_UNDEF_EXP

        End Select
        sArg = GsaComUtil.Arg(6, sGwaCommand) 'section property
        Dim bthreeNode As Boolean = False
        If Type.Equals(GsaComUtil.MembType.MB_BEAM_ARC) OrElse Type.Equals(GsaComUtil.MembType.MB_BEAM_RAD) _
         OrElse Type.Equals(GsaComUtil.MembType.MB_COL_ARC) OrElse Type.Equals(GsaComUtil.MembType.MB_COL_RAD) Then
            bthreeNode = True
        End If
        Dim posTopo0 As Integer = 8
        Dim posTopo1 As Integer = 9
        Dim posTopo2 As Integer = 10
        Dim PosRadi As Integer = 11
        Dim posNode As Integer = 12
        Dim posAngle As Integer = 13

        Dim posIsRls As Integer = 14
        Dim posIsOffset As Integer = 14
        Dim posRls0 As Integer = 0
        Dim posRls1 As Integer = 0
        Dim bRls As Boolean = True
        Dim posOff As Integer = 0
        Dim bOffset As Boolean = True


        If MembMat.MT_STEEL = Mat Then
            'read
            posTopo0 = 10
            posTopo1 = posTopo0 + 1
            If (bthreeNode) Then
                posTopo2 = posTopo1 + 1
            Else
                posTopo2 = posTopo1
            End If
            PosRadi = posTopo2 + iExpNode + 1
            posNode = PosRadi + 1
            posAngle = posNode + 1
            posIsRls = posAngle + 1

        ElseIf MembMat.MT_CONCRETE = Mat Then
            'read
            Dim num_arrange As Integer = CInt(GsaComUtil.Arg(8, sGwaCommand))
            posTopo0 = 10 + num_arrange
            posTopo1 = posTopo0 + 1
            If (bthreeNode) Then
                posTopo2 = posTopo1 + 1
            Else
                posTopo2 = posTopo1
            End If
            PosRadi = posTopo2 + iExpNode + 1
            posNode = PosRadi + 1
            posAngle = posNode + 1
            posIsRls = posAngle + 1

        Else
            'read
            posTopo0 = 8
            posTopo1 = posTopo0 + 1
            If (bthreeNode) Then
                posTopo2 = posTopo1 + 1
            Else
                posTopo2 = posTopo1
            End If
            PosRadi = posTopo2 + iExpNode + 1
            posNode = PosRadi + 1
            posAngle = posNode + 1
            posIsRls = posAngle + 1

        End If

        Dim i As Integer = posIsRls
        sArg = GsaComUtil.Arg(i, sGwaCommand)
        If sArg = "RLS" Then
            posRls0 = i + 1
            posRls1 = posRls0 + 1
            i = posRls1
        End If
        posIsOffset = i + 1
        posOff = posIsOffset + 1

        sArg = GsaComUtil.Arg(posTopo0, sGwaCommand)
        iTopoList.Add(CInt(sArg))

        sArg = GsaComUtil.Arg(posTopo1, sGwaCommand)
        iTopoList.Add(CInt(sArg))

        sArg = GsaComUtil.Arg(posTopo2, sGwaCommand)
        iTopoList.Add(CInt(sArg))

        For iExp As Integer = 1 To iExpNode - 1
            sArg = GsaComUtil.Arg(posTopo2 + iExp, sGwaCommand)
            iTopoList.Add(CInt(sArg))
        Next

        Return Type

    End Function
    'read a GSA 1D element
    Public Function IsExplicit(ByVal iEle As Integer) As Boolean
        Dim mType As MembType = GetMembType(iEle)
        If mType.Equals(GsaComUtil.MembType.MB_BEAM_EXP) OrElse mType.Equals(GsaComUtil.MembType.MB_COL_EXP) OrElse mType.Equals(GsaComUtil.MembType.MB_UNDEF_EXP) Then
            Return True
        End If
        Return False
    End Function
    Public Function IsExplicit(ByVal mType As MembType) As Boolean
        If mType.Equals(GsaComUtil.MembType.MB_BEAM_EXP) OrElse mType.Equals(GsaComUtil.MembType.MB_COL_EXP) OrElse mType.Equals(GsaComUtil.MembType.MB_UNDEF_EXP) Then
            Return True
        End If
        Return False
    End Function
    Public Function IsCurve(ByVal mType As MembType) As Boolean
        If mType.Equals(GsaComUtil.MembType.MB_BEAM_ARC) OrElse mType.Equals(GsaComUtil.MembType.MB_BEAM_RAD) _
           OrElse mType.Equals(GsaComUtil.MembType.MB_COL_ARC) OrElse mType.Equals(GsaComUtil.MembType.MB_COL_RAD) Then
            Return True
        End If
        Return False
    End Function
    Public Function IsFraming(ByVal mType As MembType) As Boolean
        If mType.Equals(GsaComUtil.MembType.MB_BEAM) OrElse mType.Equals(GsaComUtil.MembType.MB_BEAM_ARC) _
                              OrElse mType.Equals(GsaComUtil.MembType.MB_BEAM_EXP) OrElse mType.Equals(GsaComUtil.MembType.MB_BEAM_RAD) Then
            Return True
        End If
        Return False
    End Function

    Public Function Member(ByVal iElem As Integer, ByRef sName As String, ByRef iProp As Integer, _
                                ByRef uid As String, ByRef TopoList As List(Of Integer), ByRef dRadius As Double, ByRef iOrNode As Integer, ByRef dBeta As Double, _
                                ByRef sRelease0 As String, ByRef sRelease1 As String, _
                                ByRef dOffset0() As Double, ByRef dOffset1() As Double, _
                                ByRef type As MembType, ByRef mat As MembMat) As Boolean

        uid = ""
        TopoList.Clear()
        dRadius = 0.0
        iOrNode = 0
        dBeta = 0.0
        sRelease0 = ""
        sRelease1 = ""
        type = MembType.MB_UNDEF
        mat = MembMat.MT_UNDEF
        If Not Me.EntExists("MEMBER", iElem) Then
            Return False
        End If

        Dim sGwaCommand As String = ""
        Dim sArg As String

        sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,MEMB," & iElem.ToString))
        If String.IsNullOrEmpty(sGwaCommand) Then
            Return False
        End If

        ' ++
        '            MEMB.3 | num | name | colour | MT_STEEL | type | section | design | restraint | group | topo(n) | radius | node | angle |
        '                            is_rls { | rls} 
        '                            is_offset { | Ox | Ox | Oz } @end
        '            MEMB.3 | num | name | colour | MT_CONCRETE | type | section | design | num_arrange | { arrange } | 
        '                            group | topo(n) |  radius | node | angle | 
        '                            is_rls { | rls}
        '                            is_offset { | Ox | Ox | Oz } @end
        '            MEMB.3 | num | name | colour | MT_UNDEF | type | section | group | topo(n) |  radius | node | angle |
        '                            is_rls { | rls}
        '                            is_offset { | Ox | Ox | Oz } @end
        '
        '            MEMB.2 | num | name | colour | MT_STEEL | type | section | design | restraint | group | topo(2) | node | angle |
        '                            is_rls { | rls} 
        '                            is_offset { | Ox | Ox | Oz } @end
        '            MEMB.2 | num | name | colour | MT_CONCRETE | type | section | design | num_arrange | { arrange } | 
        '                            group | topo(2) | node | angle | 
        '                            is_rls { | rls}
        '                            is_offset { | Ox | Ox | Oz } @end
        '            MEMB.2 | num | name | colour | MT_UNDEF | type | section | group | topo(2) | node | angle |
        '                            is_rls { | rls}
        '                            is_offset { | Ox | Ox | Oz } @end
        '
        '            MEMB.1 | num | MT_STEEL | type | section | design | restraint | group | topo(2) | node | angle |
        '                            rls(2) | { Ox | Ox | Oz } @end
        '            MEMB.1 | num | MT_CONCRETE | type | section | design | num_arrange | { arrange } | 
        '                            group | topo(2) | node | angle | rls(2) | { Ox | Ox | Oz } @end
        '            MEMB.1 | num | MT_UNDEF | type | section | group | topo(2) | node | angle |
        '                            rls(2) | { Ox | Ox | Oz }
        '
        '            @desc                                  Member definition
        '
        '            @param
        '            num                                                       member number
        '            name                                    name
        '            colour                                   colour (ref. <a href="#colour_syntax">colour syntax</a>)
        '            type                                       member type +
        '                                                                            MB_BEAM                                          :: linear beam, topology consists two end nodes +
        '                                                                            MB_BEAM_ARC                               :: Arc with third point, topology consists two end nodes and a third node on arc  +                                                                                                      
        '                                                                            MB_BEAM_RAD                               :: Arc with radius and point a point  +
        '                                                                            MB_BEAM_EXP_#          :: Explicit beam, topology consists two end nodes and # internal nodes  +
        '                                                                                                                                               '#' indicates number of internal nodes                
        '                                                                            MB_COL                                              :: linear column +
        '                                                                            MB_COL_ARC                   :: Arc with third point, topology consists two end nodes and a third node on arc  +                                                                                                               
        '                                                                            MB_COL_RAD                   :: Arc with radius and point a point  +
        '                                                                            MB_COL_EXP_#               :: Explicit beam, topology consists two end nodes and # internal nodes  +
        '                                                                                                                                               '#' indicates number of internal nodes                
        '                                                                            MB_UNDEF                        :: undefined
        '                                                                            MB_UNDEF_ARC             :: Arc with third point, topology consists two end nodes and a third node on arc  +                                                                                                               
        '                                                                            MB_UNDEF_RAD             :: Arc with radius and point a point  +
        '                                                                            MB_UNDEF_EXP_#        :: Explicit beam, topology consists two end nodes and # internal nodes  +
        '                                                                                                                                               '#' indicates number of internal nodes                
        '            section                                 section property number
        '            design                                   design property number
        '            restraint                               restraint property number
        '            num_arrange                    number of bar arrangements
        '            {
        '            arrange                                                bar arrangements
        '            }
        '            group                                    group number
        '            topo(n)                                                topology, n numbers
        '            radius                                    radius of arc, value will be ignored for linear and explicit member types
        '            node                                     orientation node
        '            angle                                     orientation angle
        '            is_rls                                      is releases exist in member +
        '                                                                            NO_RLS                :: No Releases +
        '                                                                            NRLS      :: Releases exist
        '  rls                                                     releases at topos(2) (RRRFFF to relase translations at node) 
        '            is_offset                              is offset exist in member +
        '                                                                            NO_OFFSET        :: no offset +
        '                                                                            OFFSET                 :: offset exist
        '            Ox                                                          offset value in x direction at each node
        '            Oy                                                          offset value in y direction at each node
        '            Oz                                                           offset value in z direction at each node
        ' --


        uid = ""

        sArg = GsaComUtil.Arg(0, sGwaCommand)
        Dim idString As String = GsaComUtil.ExtractId(sArg)
        If Not String.IsNullOrEmpty(idString) Then
            uid = idString
        End If

        sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
        Debug.Assert(Integer.Equals(iElem, CInt(sArg)))

        sArg = GsaComUtil.Arg(2, sGwaCommand) 'name
        sName = sArg

        sArg = GsaComUtil.Arg(4, sGwaCommand) 'membmat
        Select Case sArg
            Case "MT_STEEL"
                mat = MembMat.MT_STEEL
            Case "MT_CONCRETE"
                mat = MembMat.MT_CONCRETE
            Case "MT_UNDEF"
                mat = MembMat.MT_UNDEF
        End Select
        Dim iExpNode As Integer = 0
        sArg = GsaComUtil.Arg(5, sGwaCommand)

        Dim ColumnStr As String = ""
        Dim ColStr As String = ""
        Dim BeamStr As String = ""

        If (sArg.Length > 12) Then
            ColumnStr = sArg.Substring(0, 13)
        End If
        If (sArg.Length > 9) Then
            ColStr = sArg.Substring(0, 10)
        End If
        If (sArg.Length > 9) Then
            BeamStr = sArg.Substring(0, 11)
        End If

        If ColumnStr.Contains("MB_COLUMN_EXP") Then
            iExpNode = Convert.ToInt16(sArg.Remove(0, 14))
            sArg = "MB_COLUMN_EXP"
        ElseIf ColStr.Contains("MB_COL_EXP") Then
            iExpNode = Convert.ToInt16(sArg.Remove(0, 11))
            sArg = "MB_COL_EXP"
        End If
        If BeamStr.Contains("MB_BEAM_EXP") Then
            iExpNode = Convert.ToInt16(sArg.Remove(0, 12))
            sArg = "MB_BEAM_EXP"
        End If


        Dim bthreeNode As Boolean = False

        Select Case sArg
            Case "MB_BEAM"
                type = MembType.MB_BEAM
            Case "MB_BEAM_ARC"
                type = MembType.MB_BEAM_ARC
            Case "MB_BEAM_RAD"
                type = MembType.MB_BEAM_RAD
            Case "MB_BEAM_EXP"
                type = MembType.MB_BEAM_EXP

            Case "MB_COLUMN"
            Case "MB_COL"
                type = MembType.MB_COL
            Case "MB_COLUMN_ARC"
            Case "MB_COL_ARC"
                type = MembType.MB_COL
            Case "MB_COLUMN_RAD"
            Case "MB_COL_RAD"
                type = MembType.MB_COL_ARC
            Case "MB_COLUMN_EXP"
            Case "MB_COL_EXP"
                type = MembType.MB_COL_EXP

            Case "MB_UNDEF"
                type = MembType.MB_UNDEF
            Case "MB_UNDEF_ARC"
                type = MembType.MB_UNDEF_ARC
            Case "MB_UNDEF_RAD"
                type = MembType.MB_UNDEF_RAD
            Case "MB_UNDEF_EXP"
                type = MembType.MB_UNDEF_EXP

        End Select

        If IsCurve(type) Then
            bthreeNode = True
        End If

        sArg = GsaComUtil.Arg(6, sGwaCommand) 'section property
        iProp = CInt(sArg)

        Dim posTopo0 As Integer = 8
        Dim posTopo1 As Integer = 9
        Dim posTopo2 As Integer = 10
        Dim PosRadi As Integer = 11
        Dim posNode As Integer = 12
        Dim posAngle As Integer = 13

        Dim posIsRls As Integer = 14
        Dim posIsOffset As Integer = 14
        Dim posRls0 As Integer = 0
        Dim posRls1 As Integer = 0
        Dim bRls As Boolean = True
        Dim posOff As Integer = 0
        Dim bOffset As Boolean = True
        Dim num_parts As Integer = (sGwaCommand.Split(New Char() {","c})).Length

        If MembMat.MT_STEEL = mat Then
            'read
            posTopo0 = 10
            posTopo1 = posTopo0 + 1
            If (bthreeNode) Then
                posTopo2 = posTopo1 + 1
            Else
                posTopo2 = posTopo1
            End If
            PosRadi = posTopo2 + iExpNode + 1
            posNode = PosRadi + 1
            posAngle = posNode + 1
            posIsRls = posAngle + 1

        ElseIf MembMat.MT_CONCRETE = mat Then
            'read
            Dim num_arrange As Integer = CInt(GsaComUtil.Arg(8, sGwaCommand))
            posTopo0 = 10 + num_arrange
            posTopo1 = posTopo0 + 1
            If (bthreeNode) Then
                posTopo2 = posTopo1 + 1
            Else
                posTopo2 = posTopo1
            End If
            PosRadi = posTopo2 + iExpNode + 1
            posNode = PosRadi + 1
            posAngle = posNode + 1
            posIsRls = posAngle + 1

        Else
            'read
            posTopo0 = 8
            posTopo1 = posTopo0 + 1
            If (bthreeNode) Then
                posTopo2 = posTopo1 + 1
            Else
                posTopo2 = posTopo1
            End If
            PosRadi = posTopo2 + iExpNode + 1
            posNode = PosRadi + 1
            posAngle = posNode + 1
            posIsRls = posAngle + 1

        End If

        Dim i As Integer = posIsRls
        sArg = GsaComUtil.Arg(i, sGwaCommand)
        If sArg = "RLS" Then
            posRls0 = i + 1
            posRls1 = posRls0 + 1
            i = posRls1
        End If
        posIsOffset = i + 1
        posOff = posIsOffset + 1

        sArg = GsaComUtil.Arg(posTopo0, sGwaCommand)
        TopoList.Add(CInt(sArg))

        sArg = GsaComUtil.Arg(posTopo1, sGwaCommand)
        TopoList.Add(CInt(sArg))

        If IsCurve(type) Then
            sArg = GsaComUtil.Arg(posTopo2, sGwaCommand)
            TopoList.Add(CInt(sArg))
        End If

        For iExp As Integer = 1 To iExpNode
            sArg = GsaComUtil.Arg(posTopo2 + iExp, sGwaCommand)
            TopoList.Add(CInt(sArg))
        Next

        'radius
        sArg = GsaComUtil.Arg(PosRadi, sGwaCommand)
        dRadius = Val(sArg)

        sArg = GsaComUtil.Arg(posNode, sGwaCommand)
        iOrNode = CInt(sArg)

        sArg = GsaComUtil.Arg(posAngle, sGwaCommand)
        dBeta = CInt(sArg)

        sArg = GsaComUtil.Arg(posIsRls, sGwaCommand)
        If sArg = "RLS" Then
            sRelease0 = GsaComUtil.Arg(posRls0, sGwaCommand)
            sRelease1 = GsaComUtil.Arg(posRls1, sGwaCommand)
        Else
            sRelease0 = "FFF"
            sRelease1 = "FFF"
        End If
        sArg = GsaComUtil.Arg(posIsOffset, sGwaCommand)
        If sArg = "OFFSET" Then
            sArg = GsaComUtil.Arg(posOff, sGwaCommand) 'offset 0, X
            dOffset0(0) = Val(sArg)
            posOff = posOff + 1
            sArg = GsaComUtil.Arg(posOff, sGwaCommand) 'offset 0, Y
            dOffset0(1) = Val(sArg)
            posOff = posOff + 1
            sArg = GsaComUtil.Arg(posOff, sGwaCommand) 'offset 0, Z
            dOffset0(2) = Val(sArg)
            posOff = posOff + 1
            sArg = GsaComUtil.Arg(posOff, sGwaCommand) 'offset 1, X
            dOffset1(0) = Val(sArg)
            posOff = posOff + 1
            sArg = GsaComUtil.Arg(posOff, sGwaCommand) 'offset 1, Y
            dOffset1(1) = Val(sArg)
            posOff = posOff + 1
            sArg = GsaComUtil.Arg(posOff, sGwaCommand) 'offset 1, Z
            dOffset1(2) = Val(sArg)
            posOff = posOff + 1
        Else
            dOffset0(0) = 0.0
            dOffset0(1) = 0.0
            dOffset0(2) = 0.0
            dOffset1(0) = 0.0
            dOffset1(1) = 0.0
            dOffset1(2) = 0.0
        End If

        Return True
    End Function
    'write a GSA Line
    Public Function SetLine(ByVal iNode0 As Integer, ByVal iNode1 As Integer, Optional ByVal iNode2 As Integer = 0, Optional ByVal dRad As Double = 0.0) As Integer
        'LINE.2 | ref | name | colour | type | 
        '   topology_1 | topology_2 | topology_3 | 
        '   radius | axis | x | y | z | xx | yy | 
        '   zz | Kx | Ky | Kz | Kxx | Kyy | Kzz | 
        '   step_definition | step_size | num_seg | step_ratio | tied_int 

        Dim iLine As Integer = HighestLine()
        iLine += 1

        Dim sGwaCommand As String = ""
        sGwaCommand += "LINE"
        sGwaCommand += "," + iLine.ToString()       'ref
        sGwaCommand += ","                          'name
        sGwaCommand += ",NO_RGB"                    'colour    


        If iNode2 = 0 Then
            sGwaCommand += ",LINE"                  'type
        Else
            sGwaCommand += ",ARC_THIRD_PT"
        End If
        sGwaCommand += "," + iNode0.ToString()      'topo1
        sGwaCommand += "," + iNode1.ToString()      'topo2
        sGwaCommand += "," + iNode2.ToString()      'topo3
        sGwaCommand += "," + dRad.ToString()        'radius
        sGwaCommand += ",GLOBAL"                    'axis

        ' Hard code these for now - 14/12/06
        sGwaCommand += ", 0, 0, 0, 0, 0, 0"         'Axis defn & constriants
        sGwaCommand += ", 0.0, 0.0, 0.0, 0.0, 0.0, 0.0," 'Kx | Ky | Kz | Kxx | Kyy | Kzz 
        sGwaCommand += "QUAD_BUILD,NUM_SEGMENTS, 1.0, 6, 1.0, NO"   'step defn, StepSize, Num_Seg, Ratio, tied_int
        m_GSAObject.GwaCommand(sGwaCommand)
        Return iLine
    End Function
    'Get a GSA line
    Public Function Line(ByRef iLine As Integer, ByRef iNode0 As Integer, ByRef iNode1 As Integer) As Boolean
        If Not LineExists(iLine) Then
            Return False
        End If
        'LINE.2 | ref | name | colour | type | 
        '   topology_1 | topology_2 | topology_3 | 
        '   radius | axis | x | y | z | xx | yy | zz | Kx | Ky | Kz | Kxx | Kyy | Kzz | step_definition | step_size | num_seg | step_ratio | tied_int 

        Dim sResult As String = CStr(m_GSAObject.GwaCommand("GET,LINE," & iLine.ToString()))
        If String.IsNullOrEmpty(sResult) Then
            Return False
        End If
        Dim sNode0 As String = GsaComUtil.Arg(5, sResult)
        Dim sNode1 As String = GsaComUtil.Arg(6, sResult)

        If True = Int32.TryParse(sNode0, iNode0) And True = Int32.TryParse(sNode1, iNode1) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function SetArea(ByRef lines As ArrayList, ByRef iProp As Integer, ByVal uid As String) As Integer

        'AREA.2 | ref | name | colour | type | span | property | group | lines | coefficient AREA.1 | ref | name | type | span | property | group | lines | coefficient 
        Dim iArea As Integer = HighestArea()
        iArea += 1
        Dim sGwaCommand As String = "AREA"
        sGwaCommand += "," + iArea.ToString()
        sGwaCommand += ","                      'name
        sGwaCommand += ",NO_RGB"                'colour   
        sGwaCommand += "," + "TWO_WAY"          'type
        sGwaCommand += "," + "0.0"              'span
        sGwaCommand += "," + iProp.ToString()   'property
        sGwaCommand += "," + "1"                'group
        sGwaCommand += ","
        For Each line As Integer In lines
            sGwaCommand += " " + line.ToString() 'lines
        Next
        sGwaCommand += ","
        sGwaCommand += ",0.0"                     'coefficient
        m_GSAObject.GwaCommand(sGwaCommand)
        If AreaExists(iArea) Then
            m_GSAObject.WriteSidTagValue("AREA", iArea, "RVT", uid)
        End If
        Return iArea
    End Function

    'write a GSA Section
    Public Function SetSection(ByVal iSec As Integer, ByVal sName As String, ByVal uid As String, ByVal usage As SectionUsage, ByVal iMat As Integer, ByVal sDesc As String, ByVal sType As String, ByVal dCost As Double, _
                                    ByVal dArea As Double, Optional ByVal bNameMap As Boolean = False, Optional ByVal bDescMap As Boolean = False) As Integer
        Dim sGwaCommand As String = ""

        If (0 = iSec) Then
            iSec = HighestSection() + 1
        End If
        Dim sid As String = "{" & GsaComUtil.SectionSid_Symbol & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        If (bDescMap) Then
            sid = "{" & "DESCRIPTION" & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        End If
        If (bNameMap) Then
            sid = "{" & "NAME" & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        End If
        'PROP_SEC | num | name | colour | mat | desc |  principal | type | cost | 
        'is_prop { | area | I11 | I22 | J | K1 | K2 } | 
        'PROP_SEC,1,Section 1,NO_RGB,STEEL,CAT%UB%UB914x419x388%19990407,NO,NA,0.000000,NO_PROP,NO_MOD_PROP

        sGwaCommand = "PROP_SEC:"
        sGwaCommand += "{RVT:" & sid & "}"
        sGwaCommand += "," & iSec.ToString()        'number
        sGwaCommand += "," & sName                  'name
        sGwaCommand += ",NO_RGB"                    'colour
        sGwaCommand += "," & iMat.ToString()        'material
        sGwaCommand += "," & sDesc                  'description
        sGwaCommand += ",NO"                        'principal
        sGwaCommand += "," & sType                  'type
        sGwaCommand += "," & dCost.ToString         'cost
        If String.Equals(sDesc.ToUpper(), "EXP") Or String.Equals(sDesc.ToUpper(), "EXPLICIT") Then
            sGwaCommand += ",PROP"                  'is_prop
            sGwaCommand += "," & dArea.ToString     'area
            sGwaCommand += "," & "0.0"              'I11
            sGwaCommand += "," & "0.0"              'I22
            sGwaCommand += "," & "0.0"              'J
            sGwaCommand += "," & "0.0"              'K1
            sGwaCommand += "," & "0.0"              'K2
            m_GSAObject.GwaCommand(sGwaCommand)
        Else
            sGwaCommand += ",NO_PROP"               'is_prop
        End If
        sGwaCommand += ",NO_MOD_PROP"               'is_mod
        m_GSAObject.GwaCommand(sGwaCommand)

        Return iSec
    End Function

    Public Function SetSectionSid(ByVal iSec As Integer, ByVal uid As String, ByVal usage As SectionUsage, Optional ByVal bNameMap As Boolean = False, Optional ByVal bDescMap As Boolean = False) As Boolean
        Dim sid As String = "{" & GsaComUtil.SectionSid_Symbol & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        If (bDescMap) Then
            sid = "{" & "DESCRIPTION" & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        End If
        If (bNameMap) Then
            sid = "{" & "NAME" & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        End If
        Return Me.SetSid("PROP_SEC", iSec, sid)
    End Function
    'read a GSA Section
    Public Function Section(ByVal iSec As Integer, ByRef sName As String, ByRef sid As String, ByRef usage As SectionUsage, ByRef iMat As Integer, ByRef sDesc As String, Optional ByRef MapOp As String = "") As Boolean
        If Not SectionExists(iSec) Then
            Return False
        End If

        Dim sGwaCommand As String = ""
        'PROP_SEC | num | name | colour | mat | desc |              principal | type    | cost | 
        'PROP_SEC	1	  name	 NO_RGB	  1	    CAT%W%W14x43%20070619	NO	    ROLLED	  0.000000	NO_PROP	NO_MOD_PROP
        sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,PROP_SEC," & iSec.ToString))
        If String.IsNullOrEmpty(sGwaCommand) Then
            Return False
        End If

        Dim sArg As String
        sArg = GsaComUtil.Arg(0, sGwaCommand)

        Dim sidList As New SortedList(Of String, String)
        Dim sid_string As String = GsaComUtil.ExtractId(sArg)
        Me.ParseNestedSid(sid_string, sidList)

        If sidList.ContainsKey(GsaComUtil.SectionSid_Usage) Then
            Dim usageString As String = sidList(GsaComUtil.SectionSid_Usage)
            If String.Equals(usageString, SectionUsage.COLUMNS.ToString()) Then
                usage = SectionUsage.COLUMNS
            ElseIf String.Equals(usageString, SectionUsage.FRAMING.ToString()) Then
                usage = SectionUsage.FRAMING
            End If
        Else
            usage = SectionUsage.INVALID ' for now
        End If

        If sidList.ContainsKey(GsaComUtil.SectionSid_Symbol) Then
            sid = sidList(GsaComUtil.SectionSid_Symbol)
        End If
        If sidList.ContainsKey("DESCRIPTION") Then
            MapOp = "DESC"
            sid = sidList("DESCRIPTION")
        End If
        If sidList.ContainsKey("NAME") Then
            MapOp = "NAME"
            sid = sidList("NAME")
        End If
        sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
        Debug.Assert(Integer.Equals(iSec, CInt(sArg)))

        sArg = GsaComUtil.Arg(2, sGwaCommand) 'name
        sName = sArg

        sArg = GsaComUtil.Arg(4, sGwaCommand) 'material
        iMat = MaterialFromString(sArg)

        sArg = GsaComUtil.Arg(5, sGwaCommand) 'description
        sDesc = sArg

        'sArg = GsaComUtil.Arg(9, sGwaCommand) 'is_prop
        'Dim sProp As String = sArg
        'If String.Equals(sProp, "PROP") Then
        '    sArg = GsaComUtil.Arg(10, sGwaCommand) 'area
        '    dArea = CDbl(sArg)
        'Else
        '    dArea = 0.0
        'End If

        Return True
    End Function
    Function SectionUsageType(ByVal strEnt As String, ByVal iSecNum As Integer, ByVal strVert As String, ByVal strInc As String, ByVal bFromMemeb As Boolean) As SectionUsage

        Dim iHgstelem As Integer = HighestEnt(strEnt)
        Dim iEnt As Integer
        Dim iProp As Integer = 1, uID As String = ""
        Dim iOrNode As Integer = 0, dBeta As Double = 0
        Dim sRelease0 As String = "", sRelease1 As String = ""
        Dim dOffset0() As Double = {0.0, 0.0, 0.0}, dOffset1() As Double = {0.0, 0.0, 0.0}
        Dim mtype As MembType = MembType.MB_UNDEF
        Dim sName As String = ""
        Dim x As Double = 0.0, y As Double = 0.0, z As Double = 0.0
        Dim x1 As Double = 0.0, y1 As Double = 0.0, z1 As Double = 0.0
        Dim sEleList As String = Nothing
        Dim dFacetval As Double = 0
        Dim iGeom As Integer = 0
        Dim dRadius As Double = 0
        Dim iTopo2 As Integer = 0
        Dim Mat As GsaComUtil.MembMat
        Dim bFram As Boolean = False
        Dim bColm As Boolean = False
        Dim iResult As Integer = 0
        Dim bOut As Boolean = False
        Dim iTopoList As New List(Of Integer)
        For iEnt = 1 To iHgstelem
            If "MEMBER" = strEnt Then
                bOut = Member(iEnt, sName, iProp, uID, iTopoList, dRadius, iOrNode, dBeta, sRelease0, sRelease1, dOffset0, dOffset1, mtype, Mat)
                If Not bOut Then
                    Continue For
                End If
            Else
                bOut = Elem1d(iEnt, iProp, uID, iTopoList, iOrNode, dBeta, sRelease0, sRelease1, dOffset0, dOffset1, "")
                If Not bOut Then
                    Continue For
                End If
            End If
            If (iProp <> iSecNum) Then
                Continue For
            End If
            'if option = Select Revit member type from GSA Member type.
            If "MEMBER" = strEnt Then
                If (bFromMemeb) Then
                    If IsFraming(mtype) Then
                        bFram = True
                        Continue For
                    Else
                        bColm = True
                        Continue For
                    End If
                End If
            End If
            'Things can be too complex if explicit element is having both vertical ,horizontal,inclined element
            Dim sNodeCord() As Double = ExtractNodeCoor(iTopoList.Item(0).ToString())
            Dim eNodeCord() As Double = ExtractNodeCoor(iTopoList.Item(1).ToString())
            Dim bVertical As Boolean = IsVertical(sNodeCord, eNodeCord)
            Dim bHoriZontal As Boolean = IsHorizontal(sNodeCord, eNodeCord)

            If IsCurve(mtype) Then
                Dim eMidCord() As Double = ExtractNodeCoor(iTopoList.Item(2).ToString())
                bVertical = IsInVerticalPlane(sNodeCord, eNodeCord, eMidCord)
                bHoriZontal = IsInHorizontalPlane(sNodeCord, eNodeCord, eMidCord)
            End If

            'What will come first will be considered.Later we can think how to handle this
            If IsExplicit(mtype) Then
                For iLst As Integer = 0 To iTopoList.Count - 2
                    Dim sNodeCord_i() As Double = ExtractNodeCoor(iTopoList.Item(iLst).ToString())
                    Dim sNodeCord_j() As Double = ExtractNodeCoor(iTopoList.Item(iLst + 1).ToString())
                    bVertical = IsVertical(sNodeCord_i, sNodeCord_j)
                    bHoriZontal = IsHorizontal(sNodeCord_i, sNodeCord_j)
                    Exit For
                Next iLst
            End If

            If bVertical Then
                If strVert.Contains("Columns") Then
                    bColm = True
                Else
                    bFram = True
                End If
            Else
                If bHoriZontal Then
                    'horizontal
                    bFram = True
                Else
                    'inclined
                    If strInc.Contains("Columns") Then
                        bColm = True
                    Else
                        bFram = True
                    End If
                End If
            End If

        Next
        Dim iFram As Integer = 1
        Dim iColm As Integer = 2
        If (bFram) Then
            iResult = iResult Or iFram
        End If
        If (bColm) Then
            iResult = iResult Or iColm
        End If
        Return CType(iResult, SectionUsage)
    End Function
    Public Shared Function IsIncline(ByVal startpoint() As Double, ByVal EndPoint() As Double) As Boolean
        Dim vect() As Double = {EndPoint(0) - startpoint(0), EndPoint(1) - startpoint(1), EndPoint(2) - startpoint(2)}
        Dim denom As Double = (Math.Sqrt(Math.Pow(vect(0), 2) + Math.Pow(vect(1), 2) + Math.Pow(vect(2), 2)))
        Dim alpha As Double = Abs(vect(0) / denom)
        Dim beta As Double = Abs(vect(1) / denom)
        Dim gamma As Double = Abs(vect(2) / denom)
        If alpha > 0.0001 OrElse beta > 0.0001 OrElse gamma > 0.0001 Then
            Return True
        End If
        Return False
    End Function
    Public Shared Function IsInHorizontalPlane(ByVal startpoint() As Double, ByVal EndPoint() As Double, ByVal MidPoint() As Double) As Boolean
        Dim Vec1 As Double() = {MidPoint(0) - startpoint(0), MidPoint(1) - startpoint(1), MidPoint(2) - startpoint(2)}
        Dim Vec2 As Double() = {MidPoint(0) - EndPoint(0), MidPoint(1) - EndPoint(1), MidPoint(2) - EndPoint(2)}

        Dim x1 As Double = Vec1(0)
        Dim y1 As Double = Vec1(1)
        Dim z1 As Double = Vec1(2)

        Dim x2 As Double = Vec1(0)
        Dim y2 As Double = Vec1(1)
        Dim z2 As Double = Vec1(2)

        Dim VecCross As Double() = {(y1 * z1 - y2 * z2), -(x2 * z2 - x2 * z1), (x1 * y1 - y1 * y2)}
        Dim bVert As Boolean = IsVertical(VecCross, MidPoint)
        Return bVert

    End Function
    Public Shared Function IsInVerticalPlane(ByVal startpoint() As Double, ByVal EndPoint() As Double, ByVal MidPoint() As Double) As Boolean
        Dim Vec1 As Double() = {MidPoint(0) - startpoint(0), MidPoint(1) - startpoint(1), MidPoint(2) - startpoint(2)}
        Dim Vec2 As Double() = {MidPoint(0) - EndPoint(0), MidPoint(1) - EndPoint(1), MidPoint(2) - EndPoint(2)}

        Dim x1 As Double = Vec1(0)
        Dim y1 As Double = Vec1(1)
        Dim z1 As Double = Vec1(2)

        Dim x2 As Double = Vec1(0)
        Dim y2 As Double = Vec1(1)
        Dim z2 As Double = Vec1(2)

        Dim VecCross As Double() = {(y1 * z1 - y2 * z2), -(x2 * z2 - x2 * z1), (x1 * y1 - y1 * y2)}
        Dim bVert As Boolean = IsVertical(VecCross, MidPoint)
        Return Not bVert

    End Function
    Public Shared Function IsInclined(ByVal startpoint() As Double, ByVal EndPoint() As Double) As Boolean
        Dim Vec As Double() = {EndPoint(0) - startpoint(0), EndPoint(1) - startpoint(1), EndPoint(2) - startpoint(2)}
        Dim denom As Double = Math.Sqrt(Math.Pow(Vec(0), 2) + Math.Pow(Vec(1), 2) + Math.Pow(Vec(2), 2))
        Dim alpha As Double = Abs(Vec(0) / denom)
        Dim beta As Double = Abs(Vec(1) / denom)
        Dim gamma As Double = Abs(Vec(2) / denom)
        If alpha > 0.0001 OrElse beta > 0.0001 OrElse gamma > 0.0001 Then
            Return True
        End If
        Return False
    End Function
    Public Shared Function IsVertical(ByVal startpoint() As Double, ByVal EndPoint() As Double) As Boolean

        Dim bX As Boolean = Utils.IsApproxEqual(startpoint(0), EndPoint(0), 0.001)
        Dim bY As Boolean = Utils.IsApproxEqual(startpoint(1), EndPoint(1), 0.001)
        If (bX And bY) Then
            Return True
        End If
        Return False
    End Function
    Public Shared Function IsHorizontal(ByVal startpoint() As Double, ByVal EndPoint() As Double) As Boolean

        Dim bX As Boolean = True ' Utils.IsApproxEqual(startpoint(0), EndPoint(0), 0.001)
        Dim bY As Boolean = Utils.IsApproxEqual(startpoint(2), EndPoint(2), 0.001)
        If (bX And bY) Then
            Return True
        End If
        Return False
    End Function
    Private Function MaterialFromString(ByRef mat As String) As Integer
        Dim iMat As Integer = 0
        If Integer.TryParse(mat, iMat) Then
            Return iMat
        End If

        Select Case mat
            Case "STEEL"
                iMat = CInt(GsaComUtil.Mat.STEEL)
            Case "CONC_SHORT"
                iMat = CInt(GsaComUtil.Mat.CONC_SHORT)
            Case "CONC_LONG"
                iMat = CInt(GsaComUtil.Mat.CONC_LONG)
            Case "ALUMINUM"
                iMat = CInt(GsaComUtil.Mat.ALUMINIUM)
        End Select

        Return iMat
    End Function
    'read a GSA Section
    Public Function Section(ByVal iSec As Integer, ByRef sName As String, ByRef sid As String, ByRef iMat As Integer, ByRef sDesc As String) As Boolean
        Dim usage As SectionUsage
        Dim ret As Boolean = Section(iSec, sName, sid, usage, iMat, sDesc)
        Return ret
    End Function

    Public Function Set2dProp(ByVal sName As String, ByVal dThick As Double, ByVal eType As ElemType) As Integer
        'PROP_2D.2 | num | name | colour | axis | mat | type | thick | mass | bending

        Dim iProp As Integer = HighestProp2d() + 1
        Dim sType As String = Me.ElemTypeString(eType)
        Dim sGwaCommand As String = "PROP_2D,"
        sGwaCommand += iProp.ToString() + ","
        sGwaCommand += sName + ","
        sGwaCommand += "NO_RGB,"    'colour
        sGwaCommand += "LOCAL,"
        sGwaCommand += "99,"
        sGwaCommand += sType + ","
        sGwaCommand += dThick.ToString() + ","
        sGwaCommand += "0.0,100%"

        m_GSAObject.GwaCommand(sGwaCommand)
        Return iProp
    End Function
    'write a GSA Material
    Public Function SetMaterial(ByVal iMat As Integer, ByVal sName As String, ByVal sid As String, ByVal sDesc As String, _
                                    ByVal dE As Double, ByVal dNu As Double, ByVal dG As Double, ByVal dRho As Double, ByVal dAlpha As Double, _
                                    ByVal dYield As Double, ByVal dUltimate As Double, ByVal dEh As Double, ByVal dBeta As Double, ByVal dDamp As Double) As Integer
        Dim sGwaCommand As String = ""

        If (iMat = 0) Then
            iMat = HighestMaterial() + 1
        End If

        'MAT_ISO | num | name | desc | E | nu | G | rho | alpha | yield | ultimate | Eh | beta | damp
        'sName = sName + "{" + sid + "}"

        sGwaCommand = "MAT_ISO:"
        sGwaCommand += "{RVT:" & sid & "}"
        sGwaCommand += "," & iMat.ToString()        'number
        sGwaCommand += "," & sName                  'name
        sGwaCommand += "," & sDesc                  'description
        sGwaCommand += "," & dE.ToString
        sGwaCommand += "," & dNu.ToString
        sGwaCommand += "," & dG.ToString
        sGwaCommand += "," & dRho.ToString
        sGwaCommand += "," & dAlpha.ToString
        sGwaCommand += "," & dYield.ToString
        sGwaCommand += "," & dUltimate.ToString
        sGwaCommand += "," & dEh.ToString
        sGwaCommand += "," & dBeta.ToString
        sGwaCommand += "," & dDamp.ToString
        m_GSAObject.GwaCommand(sGwaCommand)

        Return iMat
    End Function

    'read a GSA Material
    Public Function Material(ByVal iMat As Integer, ByRef sName As String, ByRef sDesc As String, ByRef sidFromGsa As String, _
                                    ByRef dE As Double, ByRef dNu As Double, ByRef dG As Double, ByRef dRho As Double, ByRef dAlpha As Double, _
                                    ByRef dYield As Double, ByRef dUltimate As Double, ByRef dEh As Double, ByRef dBeta As Double, ByRef dDamp As Double) As Boolean
        If Not MaterialExists(iMat) Then
            Return False
        End If
        'MAT_ISO | num | name | desc | E | nu | G | rho | alpha | yield | ultimate | Eh | beta | damp |
        'is_env { | rebar | country | variant | grade | eEnergy | eCO2 | recycle | user }

        'MAT_ISO | num | name | desc | E | nu | G | rho | alpha | yield | ultimate | Eh | beta | damp |
        '   is_env { | rebar | country | variant | grade | eEnergy | eCO2 | recycle | user }

        Dim sGwaCommand As String = ""
        Dim sArg As String

        sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,MAT," & iMat.ToString))
        If String.IsNullOrEmpty(sGwaCommand) Then
            Return False
        End If
        sArg = GsaComUtil.Arg(0, sGwaCommand)
        sidFromGsa = GsaComUtil.ExtractId(sArg) ' sid
        sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
        Debug.Assert(Integer.Equals(iMat, CInt(sArg)))

        sArg = GsaComUtil.Arg(2, sGwaCommand) 'name
        sName = sArg
        sArg = GsaComUtil.Arg(3, sGwaCommand) 'description
        sDesc = sArg
        sArg = GsaComUtil.Arg(4, sGwaCommand)
        dE = Val(sArg)
        sArg = GsaComUtil.Arg(5, sGwaCommand)
        dNu = Val(sArg)
        sArg = GsaComUtil.Arg(6, sGwaCommand)
        dG = Val(sArg)
        sArg = GsaComUtil.Arg(7, sGwaCommand)
        dRho = Val(sArg)
        sArg = GsaComUtil.Arg(8, sGwaCommand)
        dAlpha = Val(sArg)
        sArg = GsaComUtil.Arg(9, sGwaCommand)
        dYield = Val(sArg)
        sArg = GsaComUtil.Arg(10, sGwaCommand)
        dUltimate = Val(sArg)
        sArg = GsaComUtil.Arg(11, sGwaCommand)
        dEh = Val(sArg)
        sArg = GsaComUtil.Arg(12, sGwaCommand)
        dBeta = Val(sArg)
        sArg = GsaComUtil.Arg(13, sGwaCommand)
        dDamp = Val(sArg)
        Return True

    End Function
    Public Function Material(ByVal iMat As Integer, ByRef sName As String, ByRef sDesc As String, ByRef sid As String) As Boolean

        Dim dE As Double = 0.0, _
            dNu As Double = 0.0, _
            dG As Double = 0.0, _
            dRho As Double = 0.0, _
            dAlpha As Double = 0.0, _
            dYield As Double = 0.0, _
            dUltimate As Double = 0.0, _
            dEh As Double = 0.0, _
            dBeta As Double = 0.0, _
            dDamp As Double = 0.0

        Dim ret As Boolean = Me.Material(iMat, sName, sDesc, sid, dE, dNu, dG, dRho, dAlpha, dYield, dUltimate, dEh, dBeta, dDamp)
        Return ret
    End Function
    Public Function MaterialIsIsotropic(ByVal iMat As Integer) As Boolean
        If Not Me.MaterialExists(iMat) Then
            Return False
        End If
        Dim commandResult As String = CStr(m_GSAObject.GwaCommand("GET, MAT," & iMat))
        If String.IsNullOrEmpty(commandResult) Then
            Return False
        End If

        Dim arg As String = GsaComUtil.Arg(0, commandResult)
        If arg.Contains("MAT_ISO") Then
            Return True
        Else
            Return False
        End If
    End Function

    Function ElemTypeFromString(ByVal sKey As String) As ElemType

        ' strip the string of sid if any
        If sKey.Contains(":") Then
            sKey = sKey.Substring(0, sKey.IndexOf(":"))
        End If
        ' Get the element type corresponding to a keyword
        Dim etype As ElemType = ElemType.EL_UNDEF

        Select Case sKey
            Case "MEMBER", "MEMB"
                etype = ElemType.EL_BEAM
            Case "BEAM"
                etype = ElemType.EL_BEAM
            Case "BAR"
                etype = ElemType.EL_BAR
            Case "TIE"
                etype = ElemType.EL_TIE
            Case "STRUT"
                etype = ElemType.EL_STRUT
            Case "SPRING"
                etype = ElemType.EL_SPRING
            Case "LINK"
                etype = ElemType.EL_LINK
            Case "CABLE"
                etype = ElemType.EL_CABLE
            Case "QUAD4"
                etype = ElemType.EL_QUAD4
            Case "QUAD8"
                etype = ElemType.EL_QUAD8
            Case "TRI3"
                etype = ElemType.EL_TRI3
            Case "TRI6"
                etype = ElemType.EL_TRI6
            Case Else
                etype = ElemType.EL_UNDEF
        End Select

        Return etype
    End Function
    Function ElemTypeIsBeam(ByVal etype As ElemType) As Boolean
        Select Case etype
            Case ElemType.EL_BAR, ElemType.EL_BEAM, ElemType.EL_STRUT, ElemType.EL_TIE
                Return True
            Case Else
                Return False
        End Select

    End Function
    Function ElemTypeString(ByVal eType As ElemType) As String
        Dim sType As String = ""
        Select Case eType
            Case ElemType.EL_FLATPLATE
                sType = "PLATE"
            Case ElemType.EL_PLANESTRESS
                sType = "STRESS"
            Case Else
                sType = "PLATE"
        End Select
        Return sType
    End Function
    Public Sub SetGsaModelUnits(ByVal units As GsaComUtil.Units)
        Select Case units
            Case GsaComUtil.Units.IMPERIAL
                m_GSAObject.GwaCommand("UNIT_DATA,FORCE,lbf")
                m_GSAObject.GwaCommand("UNIT_DATA,LENGTH,ft") 'used for section property unit conversion: A, I...
                m_GSAObject.GwaCommand("UNIT_DATA,DISP,in") 'Gen_SectionMatchDesc uses the DISP units for interpreting general section dimensions
                m_GSAObject.GwaCommand("UNIT_DATA,SECTION,in")
                m_GSAObject.GwaCommand("UNIT_DATA,MASS,lb")
                m_GSAObject.GwaCommand("UNIT_DATA,TIME,s")
                m_GSAObject.GwaCommand("UNIT_DATA,TEMP,°F")
                m_GSAObject.GwaCommand("UNIT_DATA,STRESS,kip/in²")
                m_GSAObject.GwaCommand("UNIT_DATA,ACCEL,ft/s²")
            Case GsaComUtil.Units.METRIC
                m_GSAObject.GwaCommand("UNIT_DATA,FORCE,N")
                m_GSAObject.GwaCommand("UNIT_DATA,LENGTH,m") 'used for section property unit conversion: A, I...
                m_GSAObject.GwaCommand("UNIT_DATA,DISP,m") 'Gen_SectionMatchDesc uses the DISP units for interpreting general section dimensions
                m_GSAObject.GwaCommand("UNIT_DATA,SECTION,m")
                m_GSAObject.GwaCommand("UNIT_DATA,MASS,Kg")
                m_GSAObject.GwaCommand("UNIT_DATA,TIME,s")
                m_GSAObject.GwaCommand("UNIT_DATA,TEMP,°F")
                m_GSAObject.GwaCommand("UNIT_DATA,STRESS,N/m²")
                m_GSAObject.GwaCommand("UNIT_DATA,ACCEL,m/s²")
        End Select
    End Sub
    ''' <summary>
    ''' CAUTION: Special function for use ONLY for setting material units
    ''' </summary>
    ''' <param name="unitStrings"></param>
    ''' <remarks></remarks>
    Public Sub SetGsaTemporaryUnitsMaterial(ByRef unitStrings As String())
        ' for setting material bizzare unit factors only
        For Each unitString As String In unitStrings
            m_GSAObject.GwaCommand(unitString)
        Next
    End Sub

    Function ElemNumNode(ByVal eType As ElemType) As Integer

        ' Get the number of nodes associated with the element type

        ElemNumNode = 0

        If (eType = ElemType.EL_GROUND) Then
            ElemNumNode = 1
        ElseIf (eType = ElemType.EL_MASS) Then
            ElemNumNode = 1
        ElseIf (eType = ElemType.EL_BEAM) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_BAR) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_TIE) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_STRUT) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_SPRING) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_LINK) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_CABLE) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_SPACER) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_QUAD4) Then
            ElemNumNode = 4
        ElseIf (eType = ElemType.EL_QUAD8) Then
            ElemNumNode = 8
        ElseIf (eType = ElemType.EL_TRI3) Then
            ElemNumNode = 3
        ElseIf (eType = ElemType.EL_TRI6) Then
            ElemNumNode = 6
        End If

    End Function

    Function ListName(ByVal iList As Integer) As String
        'LIST | num | name | type | list

        If Not Me.ListExists(iList) Then
            Return Nothing
        End If
        Dim result As String = CStr(m_GSAObject.GwaCommand("GET,LIST," & iList.ToString()))
        Dim parts As String() = result.Split(New Char() {","c})
        Debug.Assert(parts.Length > 2)
        Return parts(2)

    End Function
    Private Function ListString(ByVal iList As Integer) As String
        If Not Me.ListExists(iList) Then
            Return Nothing
        End If

        Dim list As String = CStr(m_GSAObject.GwaCommand("GET, LIST," & iList.ToString()))
        Return list
    End Function
    Public Function ListTypeIsMember(ByVal iList As Integer) As Boolean
        Dim listString As String = Me.ListString(iList)
        If String.IsNullOrEmpty(listString) Then
            Return False
        End If
        If listString.Contains("MEMBER") Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Function ListTypeIsElement(ByVal iList As Integer) As Boolean
        Dim listString As String = Me.ListString(iList)
        If String.IsNullOrEmpty(listString) Then
            Return False
        End If
        If listString.Contains("ELEM") Then
            Return True
        Else
            Return False
        End If

    End Function
    Function ListItemsInList(ByVal iList As Integer) As List(Of Integer)
        Dim check As Object = m_GSAObject.GwaCommand("EXIST,LIST," & iList.ToString())
        Dim iCheck As Integer = 0

        Dim items As New List(Of Integer)
        Int32.TryParse(check.ToString(), iCheck)
        If 0 = iCheck Then
            Return items
        End If

        Dim resultObj As Object = m_GSAObject.GwaCommand("GET,LIST," & iList.ToString())
        If resultObj Is Nothing Then
            Return items
        End If

        Dim result As String = resultObj.ToString(), sEnt As String = "", listType As String = ""
        If result.Contains("ELEMENT") Then
            sEnt = "EL"
            listType = "ELEM"
        ElseIf result.Contains("MEMBER") Then
            listType = "MEMBER"
            sEnt = "MEMBER"
        Else
            Return items
        End If

        Dim parts As String() = result.Split(New Char() {","c}, StringSplitOptions.None)

        Dim nElem As Integer = CInt(m_GSAObject.GwaCommand("HIGHEST," & sEnt))
        If nElem = 0 Then
            Return items
        End If
        For i As Integer = 1 To nElem
            If Val(m_GSAObject.GwaCommand("EXIST," & sEnt & "," & i.ToString())) = 0 Then
                Continue For
            End If
            If (CBool(m_GSAObject.IsItemIncluded(listType, i, parts(4)))) Then
                items.Add(i)
            End If
        Next
        Return items

    End Function
    Function ElemDesc(ByVal eType As ElemType) As String

        ' Get a string that describes the element

        ElemDesc = "UNDEF"

        If (eType = ElemType.EL_GROUND) Then
            ElemDesc = "GROUND"
        ElseIf (eType = ElemType.EL_MASS) Then
            ElemDesc = "MASS"
        ElseIf (eType = ElemType.EL_BEAM) Then
            ElemDesc = "BEAM"
        ElseIf (eType = ElemType.EL_BAR) Then
            ElemDesc = "BAR"
        ElseIf (eType = ElemType.EL_TIE) Then
            ElemDesc = "TIE"
        ElseIf (eType = ElemType.EL_STRUT) Then
            ElemDesc = "STRUT"
        ElseIf (eType = ElemType.EL_SPRING) Then
            ElemDesc = "SPRING"
        ElseIf (eType = ElemType.EL_LINK) Then
            ElemDesc = "LINK"
        ElseIf (eType = ElemType.EL_CABLE) Then
            ElemDesc = "CABLE"
        ElseIf (eType = ElemType.EL_SPACER) Then
            ElemDesc = "SPACER"
        ElseIf (eType = ElemType.EL_QUAD4) Then
            ElemDesc = "QUAD4"
        ElseIf (eType = ElemType.EL_QUAD8) Then
            ElemDesc = "QUAD8"
        ElseIf (eType = ElemType.EL_TRI3) Then
            ElemDesc = "TRI3"
        ElseIf (eType = ElemType.EL_TRI6) Then
            ElemDesc = "TRI6"
        End If

    End Function

    'Function GsaGwaCommandObj(ByVal cGwaCommand As String) As System.Object
    '    GsaGwaCommandObj = m_GSAObject.GwaCommand(cGwaCommand)
    'End Function
    Function SectUsage(ByVal sectionNum As Integer) As SectionUsage
        Dim usageInt As Integer = m_GSAObject.SectionUsage(sectionNum, m_eSelType) ' Do this for member right now
        Dim usage As SectionUsage = CType(usageInt, SectionUsage)
        Return usage
    End Function

    Function CATSectionToSNFamily(ByVal parts As String(), ByVal usage As SectionUsage, _
                                ByRef familyName As String) As Boolean

        familyName = ""
        Dim catAbr As String = parts(1)
        If SectionUsage.FRAMING = usage Then
            Select Case catAbr
                Case "C", "HP", "L", "M", "MC", "MT", "P", "PX", "PXX", "S", "ST", "TS", "W", "WT"
                    familyName += "American_"
                Case "A-CHS250", "A-CHS350", "A-EA", "A-PFC", "A-RHS350", "A-RHS450", "A-RSJs", "A-SHS350", "A-SHS450", "A-UA", "A-UB", "A-UBP", "A-UC"
                    familyName += "Australian_"
                Case "BP", "CH", "CHS", "EA", "PFC", "RHS", "SHS", "TUB", "TUC", "UA", "UB", "UC", "UJ"
                    familyName += "British_"
                Case "UKA", "UKB", "UKBP", "UKC", "UKPFC"
                    familyName += "Corus Advance_"
                Case Else
                    familyName = ""
                    Return False
            End Select
        ElseIf SectionUsage.COLUMNS = usage Then
            Select Case catAbr
                Case "HP", "M", "P", "S", "W"
                    familyName += "American_"
                Case "A-CHS250", "A-CHS350", "A-RHS350", "A-RHS450", "A-RSJs", "A-SHS350", "A-SHS450", "A-UB", "A-UBP", "A-UC"
                    familyName += "Australian_"
                Case "BP", "CH", "CHS", "RHS", "SHS", "UB", "UC", "UJ"
                    familyName += "British_"
                Case "UKB", "UKBP", "UKC"
                    familyName += "Corus Advance_"
                Case Else
                    familyName = ""
                    Return False
            End Select
        End If
        familyName += catAbr + "_" + usage.ToString().ToLower()
        Return True
    End Function
    ' From an STD section, find out which rfa file is to be used
    ' and the dimensions of the section
    Function STDSectionToSNFamily(ByRef parts As String(), ByVal usage As SectionUsage, ByRef familyName As String) As Boolean

        Debug.Assert(String.Equals(parts(0), "STD"))
        Dim cf As Double = 3.28 / 1000

        Dim sShape As String = parts(1)
        Dim shapeSubString As String() = sShape.Split(New Char() {"("c, ")"c})
        If shapeSubString.Length > 1 Then
            sShape = shapeSubString(0)
            Select Case shapeSubString(1)
                Case "m"
                    cf = 1 / 3.28
                Case "mm"
                    cf = 1000 / 3.28
                Case "in"
                    cf = 12
                Case "ft"
                    cf = 1
                    'Case ""
            End Select
        End If
        familyName = ""

        Select Case sShape
            Case "GI", "I"
                familyName = "Generic_I_"
            Case "CH"
                familyName = "Generic_CH_"
            Case "CHS"
                familyName = "Generic_CHS_"
            Case "C"
                familyName = "Generic_C_"
            Case "RHS"
                familyName = "Oasys_Generic_RHS_"
            Case "R"
                familyName = "Generic_R_"
            Case "T"
                familyName = "Generic_T_"
            Case Else
                Return False
        End Select

        familyName += usage.ToString().ToLower()
        Return True
    End Function
    ''' <summary>
    ''' Given a descriptionm, return a map of dimension name to dimension value
    ''' </summary>
    ''' <param name="desc"></param>
    ''' <param name="dimensions"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function SectionDimensions(ByVal desc As String, ByRef dimensions As SortedList) As Boolean

        Dim parts As String() = Nothing
        parts = desc.Split(New [Char]() {"%"c, " "c}, StringSplitOptions.RemoveEmptyEntries)

        Debug.Assert(String.Equals(parts(0), "STD"))
        Dim cf As Double = 3.28 / 1000

        Dim sShape As String = parts(1)
        Dim shapeSubString As String() = sShape.Split(New Char() {"("c, ")"c})
        If shapeSubString.Length > 1 Then
            sShape = shapeSubString(0)
            Select Case shapeSubString(1)
                Case "m"
                    cf = 1 / 3.28
                Case "mm"
                    cf = 1000 / 3.28
                Case "in"
                    cf = 12
                Case "ft"
                    cf = 1
                    'Case ""
            End Select
        End If

        Select Case sShape
            Case "GI", "I"
                ExtractDimensions_I(parts, dimensions, cf)
            Case "CH"
                ExtractDimensions_CH_T(parts, dimensions, cf)
            Case "CHS"
                ExtractDimensions_CHS(parts, dimensions, cf)
            Case "C"
                ExtractDimensions_C(parts, dimensions, cf)
            Case "RHS"
                ExtractDimensions_RECT(parts, dimensions, cf)
            Case "R"
                ExtractDimensions_RECT(parts, dimensions, cf)
            Case "T"
                ExtractDimensions_CH_T(parts, dimensions, cf)
            Case Else
                Return False
        End Select
        Return True

    End Function

    Function ExtractDimensions_I(ByRef parts As String(), ByRef dimensions As SortedList, ByVal cf As Double) As Boolean
        Dim D As Double = 0.0, _
            Wt As Double = 0.0, _
            Wb As Double = 0.0, _
            Tt As Double = 0.0, _
            Tb As Double = 0.0, _
            t As Double = 0.0

        Dim b1, b2, b3, b4, b5, b6 As Boolean
        Select Case parts(1)
            Case "GI"
                ' D Wt Wb t Tt Tb

                Debug.Assert(8 = parts.Length)
                b1 = Double.TryParse(parts(2), D)
                dimensions.Add("D", D * cf)

                b2 = Double.TryParse(parts(3), Wt)
                dimensions.Add("Wt", Wt * cf)

                b3 = Double.TryParse(parts(4), Wb)
                dimensions.Add("Wb", Wb * cf)

                b4 = Double.TryParse(parts(5), t)
                dimensions.Add("t", t * cf)

                b5 = Double.TryParse(parts(6), Tt)
                dimensions.Add("Tt", Tt * cf)

                b6 = Double.TryParse(parts(7), Tb)
                dimensions.Add("Tb", Tb * cf)

                Debug.Assert(b1 And b2 And b3 And b4 And b5 And b6)

            Case "I"
                ' D W t T
                'STD I(m) 0.9 0.4 2.E-002 3.E-002
                Debug.Assert(6 = parts.Length)
                b1 = Double.TryParse(parts(2), D)
                dimensions.Add("D", D * cf)

                b2 = Double.TryParse(parts(3), Wt)
                dimensions.Add("Wt", Wt * cf)
                dimensions.Add("Wb", Wt * cf)

                b3 = Double.TryParse(parts(4), t)
                dimensions.Add("t", t * cf)

                b4 = Double.TryParse(parts(5), Tt)
                dimensions.Add("Tt", Tt * cf)
                dimensions.Add("Tb", Tt * cf)

                Debug.Assert(b1 And b2 And b3 And b4)
        End Select
        Return True
    End Function
    Function ExtractDimensions_CH_T(ByRef parts As String(), ByRef dimensions As SortedList, ByVal cf As Double) As Boolean

        Dim D As Double = 0.0, _
            W As Double = 0.0, _
            T As Double = 0.0, _
            Tt As Double = 0.0
        'STD CH(m) 0.5 0.25 2.E-002 3.E-002
        'STD T(m) 0.5 0.25 3.E-002 2.E-002

        Dim b1, b2, b3, b4 As Boolean
        Debug.Assert(6 = parts.Length)

        b1 = Double.TryParse(parts(2), D)
        dimensions.Add("D", D * cf)

        b2 = Double.TryParse(parts(3), W)
        dimensions.Add("W", W * cf)

        b3 = Double.TryParse(parts(4), T)
        dimensions.Add("T", T * cf)

        b4 = Double.TryParse(parts(5), Tt)
        dimensions.Add("t", Tt * cf)

        Debug.Assert(b1 And b2 And b3 And b4)
        Return True

    End Function
    Function ExtractDimensions_CHS(ByRef parts As String(), ByRef dimensions As SortedList, ByVal cf As Double) As Boolean

        Dim D As Double = 0.0, _
            t As Double = 0.0
        'STD CHS(m) 0.25 1.E-002

        Dim b1, b2 As Boolean
        Debug.Assert(4 = parts.Length)

        b1 = Double.TryParse(parts(2), D)
        dimensions.Add("D", D * cf)

        b2 = Double.TryParse(parts(3), t)
        dimensions.Add("t", t * cf)

        Debug.Assert(b1 And b2)
        Return True

    End Function
    Function ExtractDimensions_C(ByRef parts As String(), ByRef dimensions As SortedList, ByVal cf As Double) As Boolean
        Dim D As Double = 0.0

        Dim b1 As Boolean
        Debug.Assert(3 = parts.Length)

        b1 = Double.TryParse(parts(2), D)
        dimensions.Add("D", D * cf)

        Debug.Assert(b1)
        Return True

    End Function
    Function ExtractDimensions_RECT(ByRef parts As String(), ByRef dimensions As SortedList, ByVal cf As Double) As Boolean

        Dim D As Double = 0.0, _
            W As Double = 0.0, _
            T As Double = 0.0, _
            Tt As Double = 0.0
        'STD RHS(m) 0.25 0.3 3.E-002 3.E-002

        Dim b1, b2, b3, b4 As Boolean
        Debug.Assert(6 = parts.Length Or 4 = parts.Length)

        b1 = Double.TryParse(parts(2), D)
        dimensions.Add("D", D * cf)

        b2 = Double.TryParse(parts(3), W)
        dimensions.Add("W", W * cf)

        Dim sShape As String = parts(1)
        Dim shapeSubString As String() = sShape.Split(New [Char]() {"("c, ")"c})
        Debug.Assert(shapeSubString.Length > 0)

        If (String.Equals(shapeSubString(0), "RHS")) Then
            b3 = Double.TryParse(parts(4), T)
            dimensions.Add("T", T * cf)

            b4 = Double.TryParse(parts(5), Tt)
            dimensions.Add("t", Tt * cf)
        End If

        'Debug.Assert(b1 And b2 And b3 And b4)
        Return True

    End Function

    Function SetSid(ByVal keyword As String, ByVal record As Integer, ByVal sid As String) As Boolean
        Debug.Assert(Not String.IsNullOrEmpty(keyword) And Not String.IsNullOrEmpty(sid) And Not (0 = record))

        ' ensure record exists
        Dim iCheck As Integer = CInt(m_GSAObject.GwaCommand("EXIST," & keyword & "," & record.ToString()))
        If 1 <> iCheck Then
            Return False
        End If
        m_GSAObject.WriteSidTagValue(keyword, record, "RVT", sid)
        Return True

    End Function

    ''' <summary>
    ''' Fetches value associated with the RVT key from the sid of the record
    ''' </summary>
    ''' <param name="keyword"></param>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetSid(ByRef keyword As String, ByRef record As Integer) As String

        Debug.Assert(Not String.IsNullOrEmpty(keyword) AndAlso Not 0 = record)
        Dim iCheck As Integer = CInt(m_GSAObject.GwaCommand("EXIST," & keyword & "," & record.ToString()))
        If iCheck <> 1 Then
            Return Nothing
        End If
        Return m_GSAObject.GetSidTagValue(keyword, record, "RVT")

    End Function
    Private Function GetSidMaterial(ByRef keyword As String, ByRef record As Integer) As String

        Debug.Assert(Not String.IsNullOrEmpty(keyword))
        Dim iCheck As Integer = CInt(m_GSAObject.GwaCommand("EXIST," & keyword & "," & record.ToString()))
        If iCheck <> 1 Then
            Return Nothing
        End If
        Return m_GSAObject.GetSidTagValue(keyword, record, "RVT")

    End Function

    Function ParseModelSid(ByRef sids As SortedList(Of String, String)) As Boolean
        'Dim oSid As Object = m_GSAObject.GwaCommand("GET,SID")
        Dim sid_string As String = m_GSAObject.GetSidTagValue("SID", 1, "RVT")
        If String.IsNullOrEmpty(sid_string) Then
            'Debug.Assert(False)
            Return False
        End If
        Return Me.ParseNestedSid(sid_string, sids)

    End Function
    'Function ParseSectionSid(ByVal iSec As Integer, ByRef sids As SortedList) As Boolean

    '    Dim sid_string As String = m_GSAObject.GetSidTagValue("SEC_BEAM", iSec, "RVT")
    '    If String.IsNullOrEmpty(sid_string) Then
    '        Return False
    '    End If
    '    Return Me.ParseNestedSid(sid_string, sids)

    'End Function
    Function ParseNestedSid(ByRef sid_string As String, ByRef sids As SortedList(Of String, String)) As Boolean
        'sid is of format {RVT:{key1:value1}{key2:value2}...}

        'sid = sid.Substring(5)
        'sid = sid.Remove(sid.Length - 1)
        'Dim params As New SortedList

        Dim parts As String() = sid_string.Split(New Char() {"{"c, "}"c}, System.StringSplitOptions.RemoveEmptyEntries)
        If parts Is Nothing Then
            Return False
        End If

        For Each s As String In parts
            Dim pair As String() = s.Split(New Char() {":"c})
            If pair.Length <> 2 Then
                Continue For
            End If
            sids(pair(0)) = pair(1)
        Next
        Return True

    End Function
    Sub SetModelSid(ByRef sid As String)
        m_GSAObject.GwaCommand("SID," & sid)
    End Sub
    ''' <summary>
    ''' Highest record number for a given module
    ''' </summary>
    ''' <param name="keyword"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function HighestRecord(ByRef keyword As String) As Integer
        Dim command As String = "HIGHEST," + keyword
        Dim obj As Object = m_GSAObject.GwaCommand(command)
        Dim nRecord As Integer = CType(obj, Integer)
        Return nRecord
    End Function
    Function HighestGridPlane() As Integer
        Return Me.HighestRecord("GRID_PLANE")
    End Function
    Function HighestGridLine() As Integer
        Return Me.HighestRecord("GRID_LINE")
    End Function
    Function HighestNode() As Integer
        Return Me.HighestRecord("NODE")
    End Function
    Function HighestLine() As Integer
        Return Me.HighestRecord("LINE")
    End Function
    Function HighestArea() As Integer
        Return Me.HighestRecord("AREA")
    End Function
    Function HighestSection() As Integer
        Return Me.HighestRecord("PROP_SEC")
    End Function
    Function HighestProp2d() As Integer
        Return Me.HighestRecord("PROP_2D")
    End Function
    Function HighestMaterial() As Integer
        Return Me.HighestRecord("MAT")
    End Function
    Function HighestEnt(ByRef ent As String) As Integer
        Return Me.HighestRecord(ent)
    End Function
    Function HighestList() As Integer
        Return Me.HighestRecord("LIST")
    End Function
    ''' <summary>
    ''' Does record exist for keyword
    ''' </summary>
    ''' <param name="keyword"></param>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ModuleRecordExists(ByRef keyword As String, ByVal record As Integer) As Boolean
        Dim command As String = "EXIST," + keyword + "," + record.ToString()
        Dim obj As Object = m_GSAObject.GwaCommand(command)
        Dim bExist As Boolean = False
        Dim iResult As Integer = CType(obj, Integer)
        If 0 = iResult Then
            Return False
        Else
            Return True
        End If

    End Function
    Function SectionExists(ByVal iSec As Integer) As Boolean
        Return Me.ModuleRecordExists("PROP_SEC", iSec)
    End Function
    Function MaterialExists(ByVal iMat As Integer) As Boolean
        Return Me.ModuleRecordExists("MAT", iMat)
    End Function
    Function GridPlaneExists(ByVal iGridPl As Integer) As Boolean
        Return Me.ModuleRecordExists("GRID_PLANE", iGridPl)
    End Function
    Function GridLineExists(ByVal iGridLine As Integer) As Boolean
        Return Me.ModuleRecordExists("GRID_LINE", iGridLine)
    End Function
    Function LineExists(ByRef iLine As Integer) As Boolean
        Return Me.ModuleRecordExists("LINE", iLine)
    End Function
    Function AreaExists(ByRef iArea As Integer) As Boolean
        Return Me.ModuleRecordExists("AREA", iArea)
    End Function
    Function EntExists(ByRef ent As String, ByVal iEnt As Integer) As Boolean
        Return Me.ModuleRecordExists(ent, iEnt)
    End Function
    Function ListExists(ByRef iList As Integer) As Boolean
        Return Me.ModuleRecordExists("LIST", iList)
    End Function

    ''' <summary>
    ''' gets revit id for the material record
    ''' </summary>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks>No need to extract RevitID again when using this</remarks>
    Function MaterialSid(ByRef record As Integer) As String
        Return GetSidMaterial("MAT", record)
    End Function
    ''' <summary>
    ''' gets revit id for the element record
    ''' </summary>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks>No need to extract RevitID again when using this</remarks>
    Function ElemSid(ByRef record As Integer) As String
        Return GetSid("EL", record)
    End Function
    ''' <summary>
    ''' gets revit id for the grid plane record
    ''' </summary>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks>No need to extract RevitID again when using this</remarks>
    Function GridPlaneSid(ByRef record As Integer) As String
        Return GetSid("GRID_PLANE", record)
    End Function
    ''' <summary>
    ''' gets revit id for the grid line record
    ''' </summary>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks>No need to extract RevitID again when using this</remarks>
    Function GridLineSid(ByRef record As Integer) As String
        Return GetSid("GRID_LINE", record)
    End Function
    ''' <summary>
    ''' gets revit id for a member or element
    ''' </summary>
    ''' <param name="entity"></param>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function EntSid(ByRef entity As String, ByRef record As Integer) As String
        ' should be called only for members or elements
        Debug.Assert(String.Equals(entity, "EL") Or String.Equals(entity, "MEMBER"))

        Return GetSid(entity, record)
    End Function
    Function SectionSid(ByVal record As Integer) As SortedList(Of String, String)
        Dim sid As String = GetSid("PROP_SEC", record)
        Dim sidsMap As New SortedList(Of String, String)(2)
        If ParseNestedSid(sid, sidsMap) Then
            Return sidsMap
        Else
            Debug.Assert(False, "sid parsing failed for section " + record.ToString())
            Return Nothing
        End If
    End Function
    Function ElemIsSection(ByVal iElem As Integer) As Boolean
        Dim sGwaCommand As String = "GET,EL," + iElem.ToString()
        sGwaCommand = CStr(m_GSAObject.GwaCommand(sGwaCommand))
        Dim sArg As String = GsaComUtil.Arg(4, sGwaCommand)
        Dim eType As ElemType = Me.ElemTypeFromString(sArg)

        If ElemTypeIsBeamOrTruss(eType) Then
            Return True
        Else
            Return False
        End If
    End Function
    Function EntSection(ByRef entity As String, ByVal iEnt As Integer) As Integer
        Debug.Assert(String.Equals(entity, "EL") Or String.Equals(entity, "MEMBER"))
        If Not Me.EntExists(entity, iEnt) Then
            Debug.Assert(False)
            Return 0
        End If
        Dim sName As String = ""
        Dim iProp As Integer = 0
        Dim type As MembType
        Dim Mat As MembMat
        Dim Radius As Double
        Dim uid As String = "", iOrNode As Integer = 0, dBeta As Double = 0.0, _
        release0 As String = "", release1 As String = "", _
        dOffset0() As Double = {0.0, 0.0, 0.0}, dOffset1() As Double = {0.0, 0.0, 0.0}, strDummy As String = ""
        Dim iTopoList As New List(Of Integer)
        If String.Equals(entity, "EL") Then
            Me.Elem1d(iEnt, iProp, uid, iTopoList, iOrNode, dBeta, release0, release1, dOffset0, dOffset1, strDummy)
        Else
            Me.Member(iEnt, sName, iProp, uid, iTopoList, Radius, iOrNode, dBeta, release0, release1, dOffset0, dOffset1, type, Mat)
        End If
        Return iProp
    End Function
    Private Shared Function ElemTypeIsBeamOrTruss(ByVal eType As ElemType) As Boolean
        If (ElemType.EL_BEAM = eType _
            Or ElemType.EL_BAR = eType _
            Or ElemType.EL_STRUT = eType _
            Or ElemType.EL_TIE = eType) Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' returns sid from string {RVT:sid}
    ''' </summary>
    ''' <param name="sArg"></param>
    ''' <returns></returns>
    ''' <remarks>sid can be of form {tag1:{subtag1:data}{subtag2:data}}{tag2:data}</remarks>
    Public Shared Function ExtractId(ByVal sArg As String) As String

        Dim tag As String = "RVT:"
        Dim value As String = ""
        If Not sArg.Contains(tag) Then
            'Debug.Assert(False)
            Return value
        End If

        Dim pos_tag As Integer = sArg.IndexOf(tag)
        ' sid will be of form {tag1:{subtag1:data}{subtag2:data}}{tag2:data}
        Dim pos_value As Integer = pos_tag + tag.Length
        Dim nBraces As Integer = 1
        Dim sOpn As String = "{", sCls As String = "}"
        'Debug.Assert(Char.Equals(sArg.Chars(pos_value), sOpn.Chars(0)))

        Dim i As Integer = 0
        While nBraces > 0
            If Char.Equals(sArg.Chars(pos_value + i), sOpn.Chars(0)) Then
                nBraces += 1
            End If
            If Char.Equals(sArg.Chars(pos_value + i), sCls.Chars(0)) Then
                nBraces -= 1
            End If
            i += 1
        End While
        If sArg.Length < pos_value + i Then
            Debug.Assert(False) ' something's wrong
            Return value
        End If
        value = sArg.Substring(pos_value, i - 1)
        Return value
    End Function
    Public Property SelType() As EntType
        Get
            Return m_eSelType
        End Get
        Set(ByVal value As EntType)
            m_eSelType = value
        End Set
    End Property

End Class
