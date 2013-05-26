<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>tvNZB - Main</title>
    <style type="text/css">
        A
        {
          text-decoration:none;
        }
        .style2
        {
            text-align: center;
        }
        .style6
        {
            font-size: large;
        }
        .style8
        {
            color: gray;
            font-size: small;
        }
        </style>
</head>
<body>
    <form id="form1" runat="server">
    
    <div class="style2">
    
        <span class="style6">
    
    <asp:LinkButton ID="LinkButton_Home" runat="server" onclick="LinkButton_Home_Click">Home</asp:LinkButton>  &nbsp;  
    <asp:LinkButton ID="LinkButton_Config" runat="server" onclick="LinkButton_Config_Click">Config</asp:LinkButton>  &nbsp; 
    <asp:LinkButton ID="LinkButton_Log" runat="server" onclick="LinkButton_Log_Click">Log</asp:LinkButton>  
        </span>  
    <br class="style6" />
    <br />
    </div>
    
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <asp:MultiView ID="MultiView_tvNZB" runat="server">    
        <asp:View ID="View_Home" runat="server">
            
            <div align="center"> 
                <asp:GridView ID="gv" runat="server" AutoGenerateColumns="False" 
                    CellPadding="4" ForeColor="#333333" GridLines="None" 
                    OnPageIndexChanging="gv_PageIndexChanging" 
                    OnRowCancelingEdit="gv_RowCancelingEdit" onrowdatabound="gv_RowDataBound" 
                    OnRowDeleting="gv_RowDeleting" OnRowEditing="gv_RowEditing" 
                    OnRowUpdating="gv_RowUpdating">
                    <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                    <RowStyle BackColor="#EFF3FB" />
                    <Columns>
                        <asp:TemplateField HeaderText="Name" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_Name" runat="server" Text='<%#Eval("Name") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox_Name" runat="server" Text='<%#Eval("Name") %>' 
                                    Width="100%"></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Episode" ItemStyle-Width="20px">
                            <ItemTemplate>
                                <asp:Label ID="Label_LocalSeason" runat="server" 
                                    Text='<%#Eval("LocalSeason") + "x" + Eval("LocalEpisode") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox_LocalSeason" runat="server" 
                                    Text='<%#Eval("LocalSeason") %>' Width="100%"></asp:TextBox>
                                <asp:TextBox ID="TextBox_LocalEpisode" runat="server" 
                                    Text='<%#Eval("LocalEpisode") %>' Width="100%"></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Next Show" ItemStyle-Width="300px">
                            <ItemTemplate>
                                <asp:Label ID="Label_NextTitle" runat="server" 
                                    Text='<%# CheckEval(Eval("NextSeason") + "x" + Eval("NextEpisode") + " " + Eval("NextTitle")) %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:Label ID="Label_NextTitle" runat="server" 
                                    Text='<%#Eval("NextSeason") + "x" + Eval("NextEpisode") + " " + Eval("NextTitle") %>'></asp:Label>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Airtime" ItemStyle-Width="220px">
                            <ItemTemplate>
                                <asp:Label ID="Label_WeeklyAirtime" runat="server" 
                                    Text='<%#Eval("WeeklyAirtime") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:Label ID="Label_WeeklyAirtime" runat="server" 
                                    Text='<%#Eval("WeeklyAirtime") %>'></asp:Label>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Search Type" ItemStyle-Width="140px">
                            <ItemTemplate>
                                <asp:Label ID="Label_SearchBy" runat="server" Text='<%#Eval("SearchBy") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:DropDownList ID="DropDownList_SearchBy" runat="server" 
                                    SelectedValue='<%#Eval("SearchBy") %>' Width="100%">
                                    <asp:ListItem>Season x Episode</asp:ListItem>
                                    <asp:ListItem></asp:ListItem>
                                    <asp:ListItem>AbsoluteEpisode</asp:ListItem>
                                    <asp:ListItem>EpisodeTitle</asp:ListItem>
                                    <asp:ListItem>YYYY-MM-DD</asp:ListItem>
                                </asp:DropDownList>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:CommandField ShowEditButton="True" />
                        <asp:CommandField ShowDeleteButton="True" />
                    </Columns>
                    <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
                    <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
                    <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                    <AlternatingRowStyle BackColor="White" />
                </asp:GridView>
            </div>
                <br /> 
            <table align="center">
                <tr>
                    <td>
                        <b>Name:</b></td>
                    <td>
                        <asp:TextBox ID="TextBox_Name" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        <b>SearchBy:&nbsp;&nbsp;&nbsp; </b>
                    </td>
                    <td>
                        <asp:DropDownList ID="DropDownList_NewSearchBy" runat="server" Width="150px">
                            <asp:ListItem>EpisodeTitle</asp:ListItem>
                            <asp:ListItem>Season x Episode</asp:ListItem>
                            <asp:ListItem>YYYY-MM-DD</asp:ListItem>
                            <asp:ListItem>AbsoluteEpisode</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td colspan="2" style="text-align: center">
                        <br />
                        <asp:Button ID="Button_AddShow" runat="server" Text="Add Show" 
                            onclick="Button_AddShow_Click" />
                    </td>
                </tr>
            </table>
                <br />
        </asp:View>
        <asp:View ID="View_Config" runat="server" onload="View_Config_Load">
            <table align="center">
            <tr>
                <%--<td colspan="2" style="background-color:#507CD1; text-align: center;">
                    <span class="style1">tvNZB Config</span>
                </td>--%>
            </tr>
            <tr>
                <td>
                    Newzbin Username:</td>
                <td>
                    <asp:TextBox ID="TextBox_newzbinUserName" runat="server"  Width="219px"></asp:TextBox>
                </td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr>
                <td>Newzbin Password:</td>
                <td>
                    <asp:TextBox ID="TextBox_newzbinPassword" runat="server" TextMode="Password"  Width="219px"></asp:TextBox>
                </td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr>
                <td>Reenter Password:</td>
                <td>
                    <asp:TextBox ID="TextBox_newzbinPasswordReenter" runat="server" TextMode="Password"  Width="219px"></asp:TextBox>
                </td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr>
                <td>
                    Sabnzbd Server Address:</td>
                <td>
                    <asp:TextBox ID="TextBox_sabAddress" runat="server"  Width="219px"></asp:TextBox>
                </td>
                <td class="style8">
                    Ex. http://localhost:8080/sabnzbd/</td>
            </tr>
            <tr>
                <td>
                    Sabnzb Api Key:</td>
                <td>
                    <asp:TextBox ID="TextBox_sabAPI" runat="server" Width="219px"></asp:TextBox>
                </td>
                <td class="style8">
                    Ex. f9ebfbb278dc25ff8c4d025aaffb3c6e</td>
            </tr>
            <tr>
                <td colspan="2" style="text-align: center">
                    <br />
                    <asp:Button ID="Button_Config_Save" runat="server" Text="Save" 
                        onclick="Button_Config_Save_Click" />
                    <br />
                </td>
                <td style="text-align: center">
                    </td>
            </tr>
                <tr>
                    <td colspan="2" style="text-align: center">
                        <br />
                        <asp:Label ID="Label_Warning" runat="server" style="color: #FF3300" 
                            Text="Label"></asp:Label>
                    </td>
                    <td style="text-align: center">
                    </td>
                </tr>
        </table>
        </asp:View>
        <asp:View ID="View_Log" runat="server" onload="View_Log_Load">
            <div align="center">
                <br />
                <asp:Label ID="Label_log" runat="server" Text="Label"></asp:Label>
                </div>
        </asp:View>
    </asp:MultiView>    
        </ContentTemplate>
    </asp:UpdatePanel>
    
                                         
</form>
</body>
</html>