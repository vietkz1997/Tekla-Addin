
template _tmp_883
{
    name = "tpled_template1";
    type = GRAPHICAL;
    width = 112.1;
    maxheight = 120;
    columns = (4, 4);
    gap = 0;
    fillpolicy = CONTINUOUS;
    filldirection = VERTICAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 0.5;
    gridyspacing = 0.5;
    version = 3.21;
    created = "25.07.2009 10:20";
    modified = "20.05.2011 15:57";
    notes = "";

    row _tmp_913
    {
        name = "PART";
        height = 1;
        visibility = FALSE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "PART";
        sorttype = COMBINE;

        row _tmp_943
        {
            name = "SIMILAR_PART";
            height = 5;
            visibility = TRUE;
            usecolumns = TRUE;
            rule = "";
            contenttype = "SIMILAR_PART";
            sorttype = COMBINE;

            lineorarc _tmp_1005
            {
                name = "LineOrArc_i3";
                x1 = 28;
                y1 = 5;
                x2 = 0;
                y2 = 5.0000009704743;
                pen = -1;
                color = 152;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_1006
            {
                name = "LineOrArc_i4";
                x1 = 28;
                y1 = 0;
                x2 = 0;
                y2 = 9.7046720171079e-007;
                pen = -1;
                color = 152;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_1016
            {
                name = "LineOrArc_i14";
                x1 = 13;
                y1 = 1.98951966012828e-013;
                x2 = 13;
                y2 = 5.0000000000002;
                pen = -1;
                color = 152;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            valuefield _tmp_1105
            {
                name = "ValueField";
                location = (15, 1);
                formula = "GetValue(\"ASSEMBLY_POS\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 7;
                decimals = 0;
                sortdirection = ASCENDING;
                fontname = "romsim";
                fontcolor = 161;
                fonttype = 4;
                fontsize = 2.5;
                fontratio = 0.8;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            valuefield _tmp_1031
            {
                name = "NUMBER_field";
                location = (2, 1);
                formula = "GetValue(\"NUMBER\")";
                datatype = INTEGER;
                class = "";
                cacheable = TRUE;
                justify = RIGHT;
                visibility = TRUE;
                angle = 0;
                length = 4;
                decimals = 0;
                sortdirection = NONE;
                fontname = "romsim";
                fontcolor = 161;
                fonttype = 4;
                fontsize = 2.5;
                fontratio = 0.8;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = SUM;
            };

            lineorarc _tmp_1088
            {
                name = "LineOrArc";
                x1 = 0;
                y1 = 0;
                x2 = 0;
                y2 = 5;
                pen = -1;
                color = 152;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_1091
            {
                name = "LineOrArc_2";
                x1 = 0.5;
                y1 = 0;
                x2 = 0.5;
                y2 = 5;
                pen = -1;
                color = 152;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_1095
            {
                name = "LineOrArc_4";
                x1 = 28;
                y1 = 0;
                x2 = 28;
                y2 = 5;
                pen = -1;
                color = 152;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };
        };
    };
};
