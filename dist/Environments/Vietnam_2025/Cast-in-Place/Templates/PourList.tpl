
template _tmp_0
{
    name = "tpled_template1";
    type = GRAPHICAL;
    width = 200;
    maxheight = 10000;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 1;
    gridyspacing = 1;
    version = 3.21;
    created = "24.03.2015 16:39";
    modified = "06.01.2016 11:04";
    notes = "";

    header _tmp_1
    {
        name = "Header";
        height = 10;

        lineorarc _tmp_31
        {
            name = "LineOrArc_7";
            x1 = 0;
            y1 = 10;
            x2 = 200;
            y2 = 10;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_32
        {
            name = "LineOrArc_8";
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 10;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_33
        {
            name = "LineOrArc_9";
            x1 = 200;
            y1 = 0;
            x2 = 200;
            y2 = 10;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        valuefield _tmp_0
        {
            name = "POUR_LIST";
            location = (5, 5);
            formula = "GetValue(\"TranslatedText(\"albl_POUR_LIST\")\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 20;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 3;
            fontratio = 1;
            fontstyle = 1;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        lineorarc _tmp_1
        {
            name = "LineOrArc_13";
            x1 = 0;
            y1 = 0;
            x2 = 200;
            y2 = 0;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };
    };

    pageheader _tmp_2
    {
        name = "PageHeader";
        height = 5;
        outputpolicy = NONE;

        lineorarc _tmp_27
        {
            name = "LineOrArc_3";
            x1 = 0;
            y1 = 5;
            x2 = 0;
            y2 = 0;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_29
        {
            name = "LineOrArc_5";
            x1 = 200;
            y1 = 0;
            x2 = 200;
            y2 = 5;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        valuefield _tmp_1
        {
            name = "Pour_number";
            location = (2, 0);
            formula = "GetValue(\"TranslatedText(\"albl_Pour_number\")\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 13;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 1;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_3
        {
            name = "Pour_type";
            location = (26.322021484375, 0);
            formula = "GetValue(\"TranslatedText(\"albl_Pour_type\")\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 15;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 1;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_5
        {
            name = "Volume";
            location = (52.64404296875, 0);
            formula = "GetValue(\"TranslatedText(\"albl_Volume\")\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 15;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 1;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_7
        {
            name = "Material";
            location = (81.64404296875, 0);
            formula = "GetValue(\"TranslatedText(\"albl_Material\")\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 12;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 1;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_9
        {
            name = "Pour_date";
            location = (137.64404296875, 0);
            formula = "GetValue(\"TranslatedText(\"albl_Pour_date\")\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 18;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 1;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_2
        {
            name = "Actual_date";
            location = (168.64404296875, 0);
            formula = "GetValue(\"TranslatedText(\"albl_Actual_date\")\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 18;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 1;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_6
        {
            name = "Concrete_mixture";
            location = (106.64404296875, 0);
            formula = "GetValue(\"TranslatedText(\"j_d_j_Pour_concrete_mixture\")\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 17;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 1;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };
    };

    row _tmp_3
    {
        name = "POUR_OBJECT";
        height = 5;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "POUR_OBJECT";
        sorttype = COMBINE;

        row _tmp_0
        {
            name = "SumRow";
            height = 7;
            visibility = TRUE;
            usecolumns = FALSE;
            rule = "if (GetValue(\"POUR_NUMBER\")!=NextValue(\"POUR_NUMBER\")) then\r\n  Output()\r\nelse\r\n  StepOut()\r\nendif";
            contenttype = "POUR_OBJECT";
            sorttype = COMBINE;

            valuefield _tmp_2
            {
                name = "Sum_Volume";
                location = (68.3529493976008, 2.15304806806257);
                formula = "Sum(\"VOLUME_field\")";
                datatype = DOUBLE;
                class = "Volume";
                cacheable = TRUE;
                justify = RIGHT;
                visibility = TRUE;
                angle = 0;
                length = 20;
                decimals = 2;
                sortdirection = NONE;
                fontname = "Arial Narrow";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2.5;
                fontratio = 1;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = SUM;
                unit = "m3";
            };

            valuefield _tmp_11
            {
                name = "Sum";
                location = (36.2977827807516, 2.12791536940976);
                formula = "GetValue(\"TranslatedText(\"albl_Sum\")\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 18;
                decimals = 0;
                sortdirection = NONE;
                fontname = "Arial Narrow";
                fontcolor = 153;
                fonttype = 2;
                fontsize = 2.5;
                fontratio = 1;
                fontstyle = 1;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            lineorarc _tmp_15
            {
                name = "LineOrArc_6";
                x1 = 0;
                y1 = 6.99999818580383;
                x2 = 0;
                y2 = 0;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_16
            {
                name = "LineOrArc_11";
                x1 = 200;
                y1 = 6.99999636962048;
                x2 = 200;
                y2 = 0;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_1
            {
                name = "LineOrArc_12";
                x1 = 0;
                y1 = 0;
                x2 = 200;
                y2 = 0;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };
        };

        valuefield _tmp_14
        {
            name = "POUR_NUMBER";
            location = (3, 0);
            formula = "GetValue(\"POUR_NUMBER\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 13;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_17
        {
            name = "USERDEFINED.PLANNED_START_POUR_field";
            location = (137.65625, 0);
            formula = "format(GetValue(\"USERDEFINED.PLANNED_START_POUR\"),\"Date\",\"dd.mm.yyyy\",10)";
            datatype = STRING;
            class = "Date";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 20;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_16
        {
            name = "MATERIAL";
            location = (83.65625, 0);
            formula = "GetValue(\"MATERIAL\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 12;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_8
        {
            name = "CONCRETE_MIXTURE";
            location = (108.65625, 0);
            formula = "GetValue(\"CONCRETE_MIXTURE\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 17;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_13
        {
            name = "POUR_TYPE";
            location = (26.328125, 0);
            formula = "GetValue(\"POUR_TYPE\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 15;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_15
        {
            name = "VOLUME_field";
            location = (55.390625, 0);
            formula = "GetValue(\"VOLUME\")";
            datatype = DOUBLE;
            class = "Volume";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 15;
            decimals = 2;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = SUM;
            unit = "m3";
        };

        lineorarc _tmp_26
        {
            name = "LineOrArc_2";
            x1 = 0;
            y1 = 5;
            x2 = 0;
            y2 = 0;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_28
        {
            name = "LineOrArc_4";
            x1 = 200;
            y1 = 0;
            x2 = 200;
            y2 = 5;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        valuefield _tmp_4
        {
            name = "USERDEFINED.ACTUAL_START_POUR";
            location = (169.65625, 0);
            formula = "format(GetValue(\"USERDEFINED.ACTUAL_START_POUR\"),\"Date\",\"dd.mm.yyyy\",10)";
            datatype = STRING;
            class = "Date";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 20;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };
    };

    footer _tmp_5
    {
        name = "Footer";
        height = 10;

        lineorarc _tmp_24
        {
            name = "LineOrArc";
            x1 = 0;
            y1 = 0;
            x2 = 200;
            y2 = 0;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_25
        {
            name = "LineOrArc_1";
            x1 = 0;
            y1 = 10;
            x2 = 0;
            y2 = 0;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_36
        {
            name = "LineOrArc_10";
            x1 = 200;
            y1 = 10;
            x2 = 200;
            y2 = 0;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        valuefield _tmp_11
        {
            name = "Total";
            location = (36.2764851500749, 4.00157773575264);
            formula = "GetValue(\"TranslatedText(\"albl_Total\")\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 18;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 1;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_12
        {
            name = "Total_volume";
            location = (68.3613058482225, 4.00157773575264);
            formula = "Total(\"VOLUME_field\")";
            datatype = DOUBLE;
            class = "Volume";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 20;
            decimals = 2;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = SUM;
            unit = "m3";
        };
    };
};
