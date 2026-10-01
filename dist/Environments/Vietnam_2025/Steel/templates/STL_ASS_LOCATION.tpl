template _tmp_843
{
    name = "template1";
    type = GRAPHICAL;
    width = 54.6;
    maxheight = 120;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 0.5;
    gridyspacing = 0.5;
    version = 3.21;
    created = "26.12.2008 11:59";
    modified = "19.05.2011 18:30";
    notes = "";

    header _tmp_844
    {
        name = "Header";
        height = 6;

        lineorarc _tmp_845
        {
            name = "LineOrArc_i0";
            x1 = 0;
            y1 = 0;
            x2 = 54.6;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_846
        {
            name = "LineOrArc_i1";
            x1 = 0;
            y1 = 6;
            x2 = 54.6;
            y2 = 6;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        text _tmp_848
        {
            name = "Text";
            x1 = 8.731841613948;
            y1 = 1.23599999999988;
            x2 = 8.731841613948;
            y2 = 1.23599999999988;
            string = "GRID LOCATION";
            fontname = "romsim";
            fontcolor = 164;
            fonttype = 4;
            fontsize = 3;
            fontratio = 1.2;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        lineorarc _tmp_849
        {
            name = "LineOrArc_i4";
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 6;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_850
        {
            name = "LineOrArc_i5";
            x1 = 54.5960420085333;
            y1 = 6;
            x2 = 54.5960420085333;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };
    };

    row _tmp_904
    {
        name = "ASSEMBLY";
        height = 1;
        visibility = FALSE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "ASSEMBLY";
        sorttype = COMBINE;

        row _tmp_1083
        {
            name = "SIMILAR_ASSEMBLY";
            height = 6;
            visibility = TRUE;
            usecolumns = FALSE;
            rule = "";
            contenttype = "SIMILAR_ASSEMBLY";
            sorttype = COMBINE;

            lineorarc _tmp_1084
            {
                name = "LineOrArc";
                x1 = 0;
                y1 = 6;
                x2 = 54.6;
                y2 = 6;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_1085
            {
                name = "LineOrArc_1";
                x1 = 0;
                y1 = 0;
                x2 = 54.6;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_1086
            {
                name = "LineOrArc_2";
                x1 = 13.2;
                y1 = 0;
                x2 = 13.2;
                y2 = 6;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_1087
            {
                name = "LineOrArc_3";
                x1 = 0;
                y1 = 6;
                x2 = 0;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_1088
            {
                name = "LineOrArc_4";
                x1 = 54.5960420085333;
                y1 = 6;
                x2 = 54.5960420085333;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            valuefield _tmp_1089
            {
                name = "ValueField_2";
                location = (0, 1.5);
                formula = "GetValue(\"ASSEMBLY_POS\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 6;
                decimals = 0;
                sortdirection = ASCENDING;
                fontname = "romsim";
                fontcolor = 161;
                fonttype = 4;
                fontsize = 3;
                fontratio = 0.8;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            valuefield _tmp_1090
            {
                name = "ValueField_3";
                location = (13.4, 1.5);
                formula = "GetValue(\"ASSEMBLY_POSITION_CODE\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 9;
                decimals = 0;
                sortdirection = ASCENDING;
                fontname = "romsim";
                fontcolor = 161;
                fonttype = 4;
                fontsize = 3;
                fontratio = 0.8;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            valuefield _tmp_1091
            {
                name = "ValueField_4";
                location = (32.9, 1.5);
                formula = "\"EL\"+setat(format(GetValue(\"ASSEMBLY_TOP_LEVEL\"), \"Length\", \"mm\", 0),find(format(GetValue(\"ASSEMBLY_TOP_LEVEL\"), \"Length\", \"mm\", 0),\".\"),\",\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 10;
                decimals = 0;
                sortdirection = ASCENDING;
                fontname = "romsim";
                fontcolor = 161;
                fonttype = 4;
                fontsize = 3;
                fontratio = 0.8;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            valuefield _tmp_1856
            {
                name = "ValueField";
                location = (-1.11022302462516e-016, 0);
                formula = "GetValue(\"ID\")";
                datatype = INTEGER;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = FALSE;
                angle = 0;
                length = 1;
                sortdirection = ASCENDING;
                fontname = "Arial";
                fontcolor = 161;
                fonttype = 2;
                fontsize = 1;
                fontratio = 1;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };
        };
    };
};
