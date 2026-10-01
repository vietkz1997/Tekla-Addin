
template _tmp_843
{
    name = "template1";
    type = GRAPHICAL;
    width = 111;
    maxheight = 594;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 1;
    gridyspacing = 1;
    version = 3.21;
    created = "26.12.2008 11:08";
    modified = "13.06.2013 18:35";
    notes = "";

    header _tmp_844
    {
        name = "Header";
        height = 18;

        text _tmp_895
        {
            name = "Q'TY";
            x1 = 61.083620855336;
            y1 = 2.27694083564094;
            x2 = 61.083620855336;
            y2 = 2.27694083564094;
            string = "SIZE";
            fontname = "romsim";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 3;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_896
        {
            name = "DESCRIPTION";
            x1 = 23.8101820489509;
            y1 = 2.27694083564094;
            x2 = 23.8101820489509;
            y2 = 2.27694083564094;
            string = "FLOOR";
            fontname = "romsim";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 3;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = CENTERED;
            pen = -1;
        };

        text _tmp_897
        {
            name = "MEMBER LIST";
            x1 = 36.1764821623659;
            y1 = 10.7879946825696;
            x2 = 36.1764821623659;
            y2 = 10.7879946825696;
            string = "MEMBER LIST";
            fontname = "romsim";
            fontcolor = 164;
            fonttype = 4;
            fontsize = 4.5;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_898
        {
            name = "MARK";
            x1 = 2.41172709733341;
            y1 = 2.27694083564094;
            x2 = 2.41172709733341;
            y2 = 2.27694083564094;
            string = "MARK";
            fontname = "romsim";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 3;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_901
        {
            name = "REMARK";
            x1 = 90.055042707329;
            y1 = 2.27694083564094;
            x2 = 90.055042707329;
            y2 = 2.27694083564094;
            string = "REMARK";
            fontname = "romsim";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 3;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        lineorarc _tmp_2
        {
            name = "LineOrArc";
            x1 = 0;
            y1 = 0;
            x2 = 111;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_3
        {
            name = "LineOrArc_2";
            x1 = 111;
            y1 = 0;
            x2 = 111;
            y2 = 18;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_5
        {
            name = "LineOrArc_3";
            x1 = 0;
            y1 = 8;
            x2 = 111;
            y2 = 8;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_6
        {
            name = "LineOrArc_4";
            x1 = 16;
            y1 = 0;
            x2 = 16;
            y2 = 8;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_9
        {
            name = "LineOrArc_5";
            x1 = 45;
            y1 = 0;
            x2 = 45;
            y2 = 8;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_10
        {
            name = "LineOrArc_6";
            x1 = 86;
            y1 = 0;
            x2 = 86;
            y2 = 8;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_12
        {
            name = "LineOrArc_7";
            x1 = 0;
            y1 = 18;
            x2 = 111;
            y2 = 18;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_14
        {
            name = "LineOrArc_8";
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 18;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };
    };

    row _tmp_871
    {
        name = "STEEL&RC";
        height = 7;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if GetValue(\"PROFILE\") == PreviousValue(\"PROFILE\")\r\n&& GetValue(\"USERDEFINED.USER_FIELD_1\") == PreviousValue(\"USERDEFINED.USER_FIELD_1\") \r\n&& GetValue(\"MATERIAL\") == PreviousValue(\"MATERIAL\")\r\n|| GetValue(\"USERDEFINED.USER_FIELD_2\") == PreviousValue(\"USERDEFINED.USER_FIELD_2\") \r\n|| (match(GetValue(\"USERDEFINED.USER_FIELD_3\"),\"*SRC*\"))then\r\n \"  \"\r\nelse\r\nOutput()\r\nendif\r\n";
        contenttype = "PART";
        sorttype = COMBINE;

        valuefield _tmp_1038
        {
            name = "FLOOR";
            location = (17.6829494041816, 2);
            formula = "GetValue(\"USERDEFINED.USER_FIELD_2\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 10;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 3;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_15906
        {
            name = "PROFILE";
            location = (47.272029483649, 2);
            formula = "GetValue(\"PROFILE\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 14;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 3;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_2909
        {
            name = "MATERIAL";
            location = (88.2707838356857, 2);
            formula = "if GetValue(\"MATERIAL_TYPE\") == \"STEEL\" then\r\n GetValue(\"MATERIAL\")\r\nelse\r\n \"  \"\r\nendif\r\n\r\n\r\n\r\n\r\n";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 8;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 3;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_1
        {
            name = "ValueField";
            location = (1, 2);
            formula = "GetValue(\"USERDEFINED.USER_FIELD_1\")\r\n\r\n";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 6;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 3;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        lineorarc _tmp_15
        {
            name = "LineOrArc_9";
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_17
        {
            name = "LineOrArc_10";
            x1 = 16;
            y1 = 0;
            x2 = 16;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_18
        {
            name = "LineOrArc_11";
            x1 = 45;
            y1 = 0;
            x2 = 45;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_19
        {
            name = "LineOrArc_12";
            x1 = 86;
            y1 = 0;
            x2 = 86;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_20
        {
            name = "LineOrArc_13";
            x1 = 111;
            y1 = 0;
            x2 = 111;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_22
        {
            name = "LineOrArc_1";
            x1 = 0;
            y1 = 0;
            x2 = 111;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };
    };
};
