
template 
{
    name = "template_1880";
    type = GRAPHICAL;
    width = 45;
    maxheight = 300;
    columns = (1, 1);
    gap = 1;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 0.5;
    gridyspacing = 0.5;
    version = 3.21;
    created = "23.06.2011 04:22";
    modified = "11.08.2011 10:07";
    notes = "Converted template";

    header 
    {
        name = "header_1911";
        height = 4;

        text 
        {
            name = "text_1908";
            x1 = 21.5;
            y1 = 1;
            x2 = 21.5;
            y2 = 1;
            string = "ASS. MARK";
            fontname = "romsim";
            fontcolor = 164;
            fonttype = 4;
            fontsize = 2;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = 0;
            fontlinewidth = 1;
        };

        text 
        {
            name = "text_1907";
            x1 = 5;
            y1 = 1;
            x2 = 11;
            y2 = 3;
            string = "QT'Y";
            fontname = "romsim";
            fontcolor = 164;
            fonttype = 4;
            fontsize = 2;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = 0;
            fontlinewidth = 1;
        };

        lineorarc 
        {
            name = "lineorarc_1902";
            x1 = 15;
            y1 = 0;
            x2 = 15;
            y2 = 4;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        rectangle _tmp_31821
        {
            name = "Rectangle";
            x1 = 0;
            y1 = 4;
            x2 = 45;
            y2 = 0;
            filled = FALSE;
            filltype = -1;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
        };
    };

    row _tmp_61689
    {
        name = "PART";
        height = 3;
        visibility = FALSE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "PART";
        sorttype = COMBINE;

        row 
        {
            name = "SIMILAR_PART";
            height = 4;
            visibility = FALSE;
            usecolumns = FALSE;
            rule = "";
            contenttype = "SIMILAR_PART";
            sorttype = COMBINE;

            valuefield 
            {
                name = "field_ASSEMBLY_POS";
                location = (18, 1);
                formula = "GetValue(\"ASSEMBLY_POS\")";
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
                fontcolor = 164;
                fonttype = 2;
                fontsize = 2;
                fontratio = 1.305547;
                fontstyle = 0;
                fontslant = 0;
                pen = 0;
                oncombine = NONE;
                fontlinewidth = 1;
            };

            valuefield 
            {
                name = "field_NUMBER";
                location = (3.5, 1);
                formula = "GetValue(\"NUMBER\")";
                datatype = INTEGER;
                class = "";
                cacheable = TRUE;
                justify = CENTERED;
                visibility = TRUE;
                angle = 0;
                length = 4;
                decimals = 0;
                sortdirection = NONE;
                fontname = "Arial Narrow";
                fontcolor = 164;
                fonttype = 2;
                fontsize = 2;
                fontratio = 1.780292;
                fontstyle = 0;
                fontslant = 0;
                pen = 0;
                oncombine = SUM;
                fontlinewidth = 1;
            };

            lineorarc 
            {
                name = "lineorarc_1903";
                x1 = 15;
                y1 = 0;
                x2 = 15;
                y2 = 4;
                pen = 0;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            rectangle _tmp_31822
            {
                name = "Rectangle_1";
                x1 = 0;
                y1 = 4;
                x2 = 45;
                y2 = 0;
                filled = FALSE;
                filltype = -1;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
            };

            row _tmp_25535
            {
                name = "SIMILAR_PART2";
                height = 4;
                visibility = TRUE;
                usecolumns = FALSE;
                rule = "if GetValue(\"ROW_IN_PAGE\") > (int(GetValue(\"DRAWING.USERDEFINED.comment\"))*2)+2 then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif";
                contenttype = "SIMILAR_PART";
                sorttype = COMBINE;

                valuefield _tmp_25536
                {
                    name = "ValueField";
                    location = (18, 1);
                    formula = "GetValue(\"ASSEMBLY_POS\")";
                    datatype = STRING;
                    class = "";
                    cacheable = TRUE;
                    justify = LEFT;
                    visibility = TRUE;
                    angle = 0;
                    length = 15;
                    decimals = 0;
                    sortdirection = ASCENDING;
                    fontname = "romsim";
                    fontcolor = 161;
                    fonttype = 4;
                    fontsize = 2;
                    fontratio = 1;
                    fontstyle = 0;
                    fontslant = 0;
                    pen = 0;
                    oncombine = NONE;
                    fontlinewidth = 1;
                };

                valuefield _tmp_25537
                {
                    name = "ValueField_1";
                    location = (2.5, 1);
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
                    fontsize = 2;
                    fontratio = 1;
                    fontstyle = 0;
                    fontslant = 0;
                    pen = 0;
                    oncombine = SUM;
                    fontlinewidth = 1;
                };

                lineorarc _tmp_25538
                {
                    name = "LineOrArc";
                    x1 = 15;
                    y1 = 0;
                    x2 = 15;
                    y2 = 4;
                    pen = -1;
                    color = 153;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                rectangle _tmp_25539
                {
                    name = "Rectangle_2";
                    x1 = 0;
                    y1 = 4;
                    x2 = 45;
                    y2 = 0;
                    filled = FALSE;
                    filltype = -1;
                    pen = -1;
                    color = 153;
                    linetype = 1;
                    linewidth = 1;
                };
            };
        };
    };
};
