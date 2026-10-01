template _tmp_0
{
    name = "tpled_template1";
    type = GRAPHICAL;
    width = 190;
    maxheight = 297;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    fillstartfrom = TOPLEFT;
    margins = (0, 0, 0, 0);
    gridxspacing = 1;
    gridyspacing = 1;
    version = 3.6;
    created = "14.08.2018 00:30";
    modified = "20.05.2019 16:29";
    notes = "";

    row _tmp_2
    {
        name = "Assembly";
        height = 54;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "ASSEMBLY";
        sorttype = COMBINE;

        valuefield _tmp_67
        {
            name = "PROJECT.NAME_field";
            location = (25, 30.25);
            formula = "GetValue(\"PROJECT.NAME\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 40;
            decimals = 2;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_68
        {
            name = "DRAWING.TITLE3_field";
            location = (25, 12.25);
            formula = "GetValue(\"MAINPART.PROFILE\") + \" + \" + GetValue(\"MAINPART.MATERIAL\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 40;
            decimals = 2;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_69
        {
            name = "PROJECT.INFO1_field";
            location = (25, 3.25);
            formula = "GetValue(\"PROJECT.BUILDER\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 40;
            decimals = 2;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_70
        {
            name = "PJ_USERFIELD_1_field";
            location = (102, 21.25);
            formula = "GetValue(\"PROJECT.USERDEFINED.PROJECT_USERFIELD_1\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 20;
            decimals = 2;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_72
        {
            name = "PJ_USERFIELD_2_field";
            location = (102, 12.25);
            formula = "GetValue(\"PROJECT.USERDEFINED.PROJECT_USERFIELD_2\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 20;
            decimals = 2;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_73
        {
            name = "PJ_USERFIELD_3_field";
            location = (102, 3.25);
            formula = "GetValue(\"PROJECT.USERDEFINED.PROJECT_USERFIELD_3\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 20;
            decimals = 2;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_74
        {
            name = "PROJECT.OBJECT_field";
            location = (25, 21.25);
            formula = "GetValue(\"CURRENT_DRAWING.NAME_BASE\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 20;
            decimals = 2;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_75
        {
            name = "REV_CR_field";
            location = (147, 3.25);
            formula = "GetValue(\"REVISION.LAST_CREATED_BY\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 10;
            decimals = 2;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_76
        {
            name = "REV_CH_field";
            location = (147, 12.25);
            formula = "GetValue(\"REVISION.LAST_CHECKED_BY\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 10;
            decimals = 2;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_77
        {
            name = "REV_AP_field";
            location = (147, 21.25);
            formula = "GetValue(\"REVISION.LAST_APPROVED_BY\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 10;
            decimals = 2;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_78
        {
            name = "AP_field";
            location = (172, 21.25);
            formula = "GetValue(\"REVISION.LAST_DATE_APPROVED\")";
            maxnumoflines = 1;
            datatype = INTEGER;
            class = "Date";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 10;
            decimals = 0;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
            unit = "dd.mm.yyyy";
        };

        valuefield _tmp_79
        {
            name = "CH_field";
            location = (172, 12.25);
            formula = "GetValue(\"REVISION.LAST_DATE_CHECKED\")";
            maxnumoflines = 1;
            datatype = INTEGER;
            class = "Date";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 10;
            decimals = 0;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
            unit = "dd.mm.yyyy";
        };

        valuefield _tmp_80
        {
            name = "CR_field";
            location = (172, 3.25);
            formula = "GetValue(\"REVISION.LAST_DATE_CREATE\")";
            maxnumoflines = 1;
            datatype = INTEGER;
            class = "Date";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 10;
            decimals = 0;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
            unit = "dd.mm.yyyy";
        };

        valuefield _tmp_81
        {
            name = "Page_field";
            location = (141, 39.0512024706234);
            formula = "\"Qty : \" + CopyField(\"USERDEFINED.DRAWING_USERFIELD_1_FIELD\") +\" of \" + GetValue(\"MODEL_TOTAL\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 20;
            decimals = 0;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_82
        {
            name = "Report_NO";
            location = (141, 48.25);
            formula = "\"Report NO. : \" + GetValue(\"DRAWING.TITLE2\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 30;
            decimals = 2;
            sortdirection = NONE;
            fontname = "µ¸¿ò";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        picture _tmp_83
        {
            name = "Picture";
            file = "Tekla_Structures_logo2.png";
            refpoint = (1, 38.7000000000001);
            height = 12;
            width = 48;
            keepaspect = TRUE;
            fitinside = TRUE;
        };

        group _tmp_91
        {
            name = "Group";

            lineorarc _tmp_71
            {
                name = "LineOrArc_3";
                x1 = 118;
                y1 = 20;
                x2 = 118;
                y2 = 20;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_45
            {
                name = "LineOrArc_i0";
                x1 = 190;
                y1 = 36;
                x2 = 0;
                y2 = 36;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_46
            {
                name = "LineOrArc_i1";
                x1 = 190;
                y1 = 27;
                x2 = 0;
                y2 = 27;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_47
            {
                name = "LineOrArc_i2";
                x1 = 190;
                y1 = 18;
                x2 = 0;
                y2 = 18;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_48
            {
                name = "LineOrArc_i3";
                x1 = 190;
                y1 = 9;
                x2 = 0;
                y2 = 9;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_51
            {
                name = "LineOrArc_i7";
                x1 = 190;
                y1 = 45;
                x2 = 140;
                y2 = 45;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_64
            {
                name = "LineOrArc";
                x1 = 190;
                y1 = 0;
                x2 = 190;
                y2 = 54;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_65
            {
                name = "LineOrArc_1";
                x1 = 190;
                y1 = 54;
                x2 = 0;
                y2 = 54;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_49
            {
                name = "LineOrArc_i5";
                x1 = 140;
                y1 = 54;
                x2 = 140;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            text _tmp_55
            {
                name = "(INSPECTION REPORT)";
                x1 = 76.0719131614654;
                y1 = 39.324892909344;
                x2 = 76.0719131614654;
                y2 = 39.324892909344;
                string = "(INSPECTION REPORT)";
                fontname = "µ¸¿ò";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2.5;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            text _tmp_63
            {
                name = "Ä¡¼ö°Ë»çº¸°í¼­";
                x1 = 70.685210312076;
                y1 = 44.7796010648422;
                x2 = 70.685210312076;
                y2 = 44.7796010648422;
                string = "Ä¡¼ö°Ë»çº¸°í¼­";
                fontname = "µ¸¿ò";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 5;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            text _tmp_57
            {
                name = "DATE";
                x1 = 175.47829036635;
                y1 = 30.25;
                x2 = 175.47829036635;
                y2 = 30.25;
                string = "DATE";
                fontname = "µ¸¿ò";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2.5;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            text _tmp_56
            {
                name = "SIGNATURE";
                x1 = 145.24762550882;
                y1 = 30.25;
                x2 = 145.24762550882;
                y2 = 30.25;
                string = "SIGNATURE";
                fontname = "µ¸¿ò";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2.5;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            text _tmp_58
            {
                name = "INSPECTED BY";
                x1 = 105.603459972863;
                y1 = 30.25;
                x2 = 105.603459972863;
                y2 = 30.25;
                string = "INSPECTED BY";
                fontname = "µ¸¿ò";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2.5;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            lineorarc _tmp_50
            {
                name = "LineOrArc_i6";
                x1 = 170;
                y1 = 36;
                x2 = 170;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_53
            {
                name = "LineOrArc_i9";
                x1 = 96;
                y1 = 36;
                x2 = 96;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_52
            {
                name = "LineOrArc_i8";
                x1 = 21;
                y1 = 36;
                x2 = 21;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_66
            {
                name = "LineOrArc_2";
                x1 = 0;
                y1 = 54;
                x2 = 0;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            text _tmp_62
            {
                name = "CUSTOMER";
                x1 = 0.908751696065099;
                y1 = 3.25;
                x2 = 0.908751696065099;
                y2 = 3.25;
                string = "CUSTOMER";
                fontname = "µ¸¿ò";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2.5;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            text _tmp_61
            {
                name = "ITEM NO.";
                x1 = 2.7981682496608;
                y1 = 12.25;
                x2 = 2.7981682496608;
                y2 = 12.25;
                string = "ITEM NO.";
                fontname = "µ¸¿ò";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2.5;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            text _tmp_60
            {
                name = "AREA";
                x1 = 1.9308005427408;
                y1 = 21.25;
                x2 = 1.9308005427408;
                y2 = 21.25;
                string = "DWG. NO.";
                fontname = "µ¸¿ò";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2.5;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            text _tmp_59
            {
                name = "PROJECT";
                x1 = 2.7235413839891;
                y1 = 30.25;
                x2 = 2.7235413839891;
                y2 = 30.25;
                string = "PROJECT";
                fontname = "µ¸¿ò";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2.5;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            lineorarc _tmp_54
            {
                name = "LineOrArc_i10";
                x1 = 50;
                y1 = 54;
                x2 = 50;
                y2 = 36;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_84
            {
                name = "LineOrArc_i4";
                x1 = 190;
                y1 = 0;
                x2 = 0;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };
        };
    };

    row _tmp_0
    {
        name = "DRAWING";
        height = 6;
        visibility = FALSE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "DRAWING";
        sorttype = COMBINE;

        valuefield _tmp_1
        {
            name = "USERDEFINED.DRAWING_USERFIELD_1_FIELD";
            location = (2.38799999999998, 1.90878721278722);
            formula = "GetValue(\"USERDEFINED.DRAWING_USERFIELD_1\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 5;
            decimals = 2;
            sortdirection = ASCENDING;
            fontname = "fixfont";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 2.5;
            fontratio = 1.5;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };
    };
};
