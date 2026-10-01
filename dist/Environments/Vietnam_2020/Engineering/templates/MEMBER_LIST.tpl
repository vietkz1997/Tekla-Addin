template _tmp_843
{
    name = "template1";
    type = GRAPHICAL;
    width = 85;
    maxheight = 350;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    fillstartfrom = TOPLEFT;
    margins = (0, 0, 0, 0);
    gridxspacing = 0.5;
    gridyspacing = 0.5;
    version = 3.6;
    created = "26.12.2008 11:08";
    modified = "18.07.2018 09:21";
    notes = "";

    header _tmp_904
    {
        name = "Header";
        height = 18;

        group _tmp_309
        {
            name = "Group";

            text _tmp_246
            {
                name = "Text";
                x1 = 37.4711472230979;
                y1 = 2.68469132834482;
                x2 = 37.4711472230979;
                y2 = 2.68469132834482;
                string = "SIZE";
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

            text _tmp_247
            {
                name = "Text_1";
                x1 = 24.2648244761885;
                y1 = 10.9622150773881;
                x2 = 24.2648244761885;
                y2 = 10.9622150773881;
                string = "MEMBER LIST";
                fontname = "romsim";
                fontcolor = 164;
                fonttype = 4;
                fontsize = 3.5;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            text _tmp_248
            {
                name = "Text_2";
                x1 = 6.52901086880689;
                y1 = 2.68469132834482;
                x2 = 6.52901086880689;
                y2 = 2.68469132834482;
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

            text _tmp_249
            {
                name = "Text_3";
                x1 = 64.5514976657582;
                y1 = 2.68469132834482;
                x2 = 64.5514976657582;
                y2 = 2.68469132834482;
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

            lineorarc _tmp_250
            {
                name = "LineOrArc_1";
                x1 = 0;
                y1 = 8;
                x2 = 85;
                y2 = 8;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_251
            {
                name = "LineOrArc_5";
                x1 = 24;
                y1 = 0;
                x2 = 24;
                y2 = 8;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_252
            {
                name = "LineOrArc_10";
                x1 = 61;
                y1 = 0;
                x2 = 61;
                y2 = 8;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_253
            {
                name = "LineOrArc_36";
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

            lineorarc _tmp_254
            {
                name = "LineOrArc_37";
                x1 = 0;
                y1 = 18;
                x2 = 85;
                y2 = 18;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_255
            {
                name = "LineOrArc_38";
                x1 = 85;
                y1 = 0;
                x2 = 85;
                y2 = 18;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_256
            {
                name = "LineOrArc_39";
                x1 = 0;
                y1 = 0;
                x2 = 85;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };
        };
    };

    row _tmp_2136
    {
        name = "FOOTING";
        height = 7;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (match(GetValue(\"NAME\"),\"*FOOTING*\")) \r\n&&(match(GetValue(\"MATERIAL_TYPE\"),\"CONCRETE\"))  then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif\r\n";
        contenttype = "PART";
        sorttype = COMBINE;

        valuefield _tmp_2143
        {
            name = "ValueField_5";
            location = (4.41685288675138, 2);
            formula = "GetValue(\"FINISH\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
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

        valuefield _tmp_2144
        {
            name = "ValueField_6";
            location = (26.4168841933356, 2);
            formula = "\"THK\" + \" \" + int(GetValue(\"PROFILE.WIDTH\"))";
            maxnumoflines = 1;
            datatype = STRING;
            class = "Length";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
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

        group _tmp_334
        {
            name = "Group_1";

            lineorarc _tmp_260
            {
                name = "LineOrArc_1";
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

            lineorarc _tmp_264
            {
                name = "LineOrArc";
                x1 = 24;
                y1 = 0;
                x2 = 24;
                y2 = 7;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_287
            {
                name = "LineOrArc_3";
                x1 = 0;
                y1 = 0;
                x2 = 85;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_262
            {
                name = "LineOrArc_33";
                x1 = 0;
                y1 = 7;
                x2 = 85;
                y2 = 7;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_265
            {
                name = "LineOrArc_2";
                x1 = 61;
                y1 = 0;
                x2 = 61;
                y2 = 7;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_268
            {
                name = "LineOrArc_13";
                x1 = 85;
                y1 = 0;
                x2 = 85;
                y2 = 7;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };
        };
    };

    row _tmp_42
    {
        name = "EMPTY_1";
        height = 7;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (match(GetValue(\"NAME\"),\"*FOOTING*\")) \r\n&&(match(GetValue(\"MATERIAL_TYPE\"),\"CONCRETE\")\r\n&& GetValue(\"POUR_PHASE\") >= NextValue(\"POUR_PHASE\"))  then\r\n  Output()\r\nelse\r\n  StepOut()\r\nendif\r\n";
        contenttype = "PART";
        sorttype = COMBINE;

        lineorarc _tmp_43
        {
            name = "LineOrArc";
            x1 = 24;
            y1 = 0;
            x2 = 24;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_44
        {
            name = "LineOrArc_1";
            x1 = 61;
            y1 = 0;
            x2 = 61;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_45
        {
            name = "LineOrArc_2";
            x1 = 85;
            y1 = 0;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_46
        {
            name = "LineOrArc_3";
            x1 = 0;
            y1 = 7;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_47
        {
            name = "LineOrArc_5";
            x1 = 0;
            y1 = 0;
            x2 = 85;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_48
        {
            name = "LineOrArc_10";
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
    };

    row _tmp_1064
    {
        name = "SLAB";
        height = 7;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (match(GetValue(\"NAME\"),\"*SLAB*\")) \r\n&&(match(GetValue(\"MATERIAL_TYPE\"),\"CONCRETE\"))  then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif\r\n";
        contenttype = "PART";
        sorttype = COMBINE;

        valuefield _tmp_1071
        {
            name = "ValueField_3";
            location = (4.41685288675138, 2);
            formula = "GetValue(\"FINISH\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
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

        valuefield _tmp_1072
        {
            name = "ValueField_4";
            location = (26.4168841933356, 2);
            formula = "\"THK\" + \" \" + int(GetValue(\"PROFILE.WIDTH\"))";
            maxnumoflines = 1;
            datatype = STRING;
            class = "Length";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
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

        lineorarc _tmp_266
        {
            name = "LineOrArc_6";
            x1 = 24;
            y1 = 0;
            x2 = 24;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_267
        {
            name = "LineOrArc_7";
            x1 = 61;
            y1 = 0;
            x2 = 61;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        group _tmp_14
        {
            name = "Group_2";

            lineorarc _tmp_15
            {
                name = "LineOrArc_1";
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

            lineorarc _tmp_16
            {
                name = "LineOrArc";
                x1 = 24;
                y1 = 0;
                x2 = 24;
                y2 = 7;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_17
            {
                name = "LineOrArc_3";
                x1 = 0;
                y1 = 0;
                x2 = 85;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_18
            {
                name = "LineOrArc_33";
                x1 = 0;
                y1 = 7;
                x2 = 85;
                y2 = 7;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_19
            {
                name = "LineOrArc_2";
                x1 = 61;
                y1 = 0;
                x2 = 61;
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
                x1 = 85;
                y1 = 0;
                x2 = 85;
                y2 = 7;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };
        };
    };

    row _tmp_1048
    {
        name = "EMPTY_2";
        height = 7;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (match(GetValue(\"NAME\"),\"*SLAB*\")) \r\n&&(match(GetValue(\"MATERIAL_TYPE\"),\"CONCRETE\")\r\n&& GetValue(\"POUR_PHASE\") >= NextValue(\"POUR_PHASE\"))  then\r\n  Output()\r\nelse\r\n  StepOut()\r\nendif\r\n";
        contenttype = "PART";
        sorttype = COMBINE;

        lineorarc _tmp_270
        {
            name = "LineOrArc_19";
            x1 = 24;
            y1 = 0;
            x2 = 24;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_271
        {
            name = "LineOrArc_20";
            x1 = 61;
            y1 = 0;
            x2 = 61;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_272
        {
            name = "LineOrArc_21";
            x1 = 85;
            y1 = 0;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_288
        {
            name = "LineOrArc_4";
            x1 = 0;
            y1 = 7;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_289
        {
            name = "LineOrArc_9";
            x1 = 0;
            y1 = 0;
            x2 = 85;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_308
        {
            name = "LineOrArc_25";
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
    };

    row _tmp_1058
    {
        name = "WALL";
        height = 7;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (match(GetValue(\"NAME\"),\"*WALL*\")) \r\n&&(match(GetValue(\"MATERIAL_TYPE\"),\"CONCRETE\"))  then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif\r\n";
        contenttype = "PART";
        sorttype = COMBINE;

        valuefield _tmp_1060
        {
            name = "Finsh2";
            location = (4.41685288675138, 2);
            formula = "GetValue(\"FINISH\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
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

        valuefield _tmp_1061
        {
            name = "ValueField_1";
            location = (26.4168841933356, 2);
            formula = "\"THK\" + \" \" + int(GetValue(\"PROFILE.WIDTH\"))";
            maxnumoflines = 1;
            datatype = STRING;
            class = "Length";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
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

        group _tmp_28
        {
            name = "Group_3";

            lineorarc _tmp_29
            {
                name = "LineOrArc_1";
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

            lineorarc _tmp_30
            {
                name = "LineOrArc";
                x1 = 24;
                y1 = 0;
                x2 = 24;
                y2 = 7;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_31
            {
                name = "LineOrArc_3";
                x1 = 0;
                y1 = 0;
                x2 = 85;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_32
            {
                name = "LineOrArc_33";
                x1 = 0;
                y1 = 7;
                x2 = 85;
                y2 = 7;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_33
            {
                name = "LineOrArc_2";
                x1 = 61;
                y1 = 0;
                x2 = 61;
                y2 = 7;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_34
            {
                name = "LineOrArc_13";
                x1 = 85;
                y1 = 0;
                x2 = 85;
                y2 = 7;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };
        };
    };

    row _tmp_15661
    {
        name = "EMPTY_3";
        height = 7;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (match(GetValue(\"NAME\"),\"*WALL*\")) \r\n&&(match(GetValue(\"MATERIAL_TYPE\"),\"CONCRETE\"))  then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif\r\n";
        contenttype = "PART";
        sorttype = COMBINE;

        lineorarc _tmp_275
        {
            name = "LineOrArc_28";
            x1 = 24;
            y1 = 0;
            x2 = 24;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_279
        {
            name = "LineOrArc_35";
            x1 = 61;
            y1 = 0;
            x2 = 61;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_284
        {
            name = "LineOrArc_43";
            x1 = 85;
            y1 = 0;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_292
        {
            name = "LineOrArc_16";
            x1 = 0;
            y1 = 7;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_293
        {
            name = "LineOrArc_17";
            x1 = 0;
            y1 = 0;
            x2 = 85;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_296
        {
            name = "LineOrArc_14";
            x1 = 0;
            y1 = 7;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_297
        {
            name = "LineOrArc_15";
            x1 = 0;
            y1 = 0;
            x2 = 85;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_304
        {
            name = "LineOrArc_22";
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
    };

    row _tmp_18
    {
        name = "COLUMN";
        height = 7;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (match(GetValue(\"NAME\"),\"*COLUMN*\")) \r\n&&(match(GetValue(\"MATERIAL_TYPE\"),\"CONCRETE\"))  then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif\r\n";
        contenttype = "PART";
        sorttype = COMBINE;

        valuefield _tmp_19
        {
            name = "ValueField";
            location = (4.41685288675138, 1.78005376344087);
            formula = "GetValue(\"FINISH\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
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

        valuefield _tmp_20
        {
            name = "ValueField_2";
            location = (26.4168841933356, 1.78005376344087);
            formula = "int(GetValue(\"PROFILE.WIDTH\")) +\" x \"+ int(GetValue(\"PROFILE.HEIGHT\"))";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 14;
            decimals = 0;
            sortdirection = NONE;
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

        lineorarc _tmp_21
        {
            name = "LineOrArc_13";
            x1 = 24;
            y1 = 0;
            x2 = 24;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_22
        {
            name = "LineOrArc_26";
            x1 = 61;
            y1 = 0;
            x2 = 61;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_23
        {
            name = "LineOrArc_29";
            x1 = 85;
            y1 = 0;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_24
        {
            name = "LineOrArc_30";
            x1 = 0;
            y1 = 7;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_25
        {
            name = "LineOrArc_32";
            x1 = 0;
            y1 = 0;
            x2 = 85;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_26
        {
            name = "LineOrArc_33";
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
    };

    row _tmp_36
    {
        name = "EMPTY_4";
        height = 7;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (match(GetValue(\"NAME\"),\"*COLUMN*\")) \r\n&&(match(GetValue(\"MATERIAL_TYPE\"),\"CONCRETE\"))  then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif\r\n";
        contenttype = "PART";
        sorttype = COMBINE;

        lineorarc _tmp_37
        {
            name = "LineOrArc_36";
            x1 = 24;
            y1 = 0;
            x2 = 24;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_38
        {
            name = "LineOrArc_37";
            x1 = 61;
            y1 = 0;
            x2 = 61;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_39
        {
            name = "LineOrArc_38";
            x1 = 85;
            y1 = 0;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_40
        {
            name = "LineOrArc_39";
            x1 = 0;
            y1 = 7;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_41
        {
            name = "LineOrArc_41";
            x1 = 0;
            y1 = 0;
            x2 = 85;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_42
        {
            name = "LineOrArc_44";
            x1 = 0;
            y1 = 7;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_43
        {
            name = "LineOrArc_45";
            x1 = 0;
            y1 = 0;
            x2 = 85;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_44
        {
            name = "LineOrArc_47";
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
    };

    row _tmp_8531
    {
        name = "RCGIRDER";
        height = 7;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (match(GetValue(\"NAME\"),\"*GIRDER*\")) \r\n&&(match(GetValue(\"MATERIAL_TYPE\"),\"CONCRETE\"))  then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif\r\n";
        contenttype = "PART";
        sorttype = COMBINE;

        valuefield _tmp_4895
        {
            name = "finish_1";
            location = (4.41685288675138, 1.78005376344087);
            formula = "GetValue(\"FINISH\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
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

        valuefield _tmp_4897
        {
            name = "PROFILE_1";
            location = (26.4168841933356, 1.78005376344087);
            formula = "int(GetValue(\"PROFILE.WIDTH\")) +\" x \"+ int(GetValue(\"PROFILE.HEIGHT\"))";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 14;
            decimals = 0;
            sortdirection = NONE;
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

        lineorarc _tmp_274
        {
            name = "LineOrArc_27";
            x1 = 24;
            y1 = 0;
            x2 = 24;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_278
        {
            name = "LineOrArc_34";
            x1 = 61;
            y1 = 0;
            x2 = 61;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_283
        {
            name = "LineOrArc_42";
            x1 = 85;
            y1 = 0;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_290
        {
            name = "LineOrArc_11";
            x1 = 0;
            y1 = 7;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_291
        {
            name = "LineOrArc_12";
            x1 = 0;
            y1 = 0;
            x2 = 85;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_307
        {
            name = "LineOrArc_8";
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
    };

    row _tmp_15710
    {
        name = "RCBEAM";
        height = 7;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (match(GetValue(\"NAME\"),\"*BEAM*\")) \r\n&&(match(GetValue(\"MATERIAL_TYPE\"),\"CONCRETE\"))  then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif\r\n";
        contenttype = "PART";
        sorttype = COMBINE;

        valuefield _tmp_15718
        {
            name = "finsh_2";
            location = (4.41685288675138, 2);
            formula = "GetValue(\"FINISH\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
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

        valuefield _tmp_15719
        {
            name = "PROFILE_2";
            location = (26.4168841933356, 2);
            formula = "int(GetValue(\"PROFILE.WIDTH\")) +\" x \"+ int(GetValue(\"PROFILE.HEIGHT\"))";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 14;
            decimals = 0;
            sortdirection = NONE;
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

        lineorarc _tmp_276
        {
            name = "LineOrArc_31";
            x1 = 24;
            y1 = 0;
            x2 = 24;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_280
        {
            name = "LineOrArc_40";
            x1 = 61;
            y1 = 0;
            x2 = 61;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_285
        {
            name = "LineOrArc_46";
            x1 = 85;
            y1 = 0;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_294
        {
            name = "LineOrArc_18";
            x1 = 0;
            y1 = 7;
            x2 = 85;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_295
        {
            name = "LineOrArc_23";
            x1 = 0;
            y1 = 0;
            x2 = 85;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_305
        {
            name = "LineOrArc_24";
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
    };
};
