
template _tmp_883
{
    name = "tpled_template1";
    type = GRAPHICAL;
    width = 45.7143985748291;
    maxheight = 120;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 0.5;
    gridyspacing = 0.5;
    version = 3.21;
    created = "23.07.2009 11:13";
    modified = "19.05.2011 18:50";
    notes = "";

    row _tmp_942
    {
        name = "ASSEMBLY";
        height = 1;
        visibility = FALSE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "ASSEMBLY";
        sorttype = COMBINE;

        row _tmp_5348
        {
            name = "SIMILAR_ASSEMBLY";
            height = 1;
            visibility = FALSE;
            usecolumns = FALSE;
            rule = "";
            contenttype = "SIMILAR_ASSEMBLY";
            sorttype = COMBINE;

            row _tmp_1002
            {
                name = "PART";
                height = 1;
                visibility = FALSE;
                usecolumns = FALSE;
                rule = "";
                contenttype = "PART";
                sorttype = COMBINE;

                row _tmp_1032
                {
                    name = "BOLT";
                    height = 5;
                    visibility = TRUE;
                    usecolumns = FALSE;
                    rule = "";
                    contenttype = "BOLT";
                    sorttype = COMBINE;

                    valuefield _tmp_6739
                    {
                        name = "ValueField";
                        location = (45, 1);
                        formula = "GetValue(\"NUMBER\")";
                        datatype = INTEGER;
                        class = "";
                        cacheable = TRUE;
                        justify = LEFT;
                        visibility = FALSE;
                        angle = 0;
                        length = 1;
                        sortdirection = NONE;
                        fontname = "Arial";
                        fontcolor = 152;
                        fonttype = 2;
                        fontsize = 1;
                        fontratio = 1;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = SUM;
                    };

                    valuefield _tmp_6997
                    {
                        name = "length";
                        location = (45, 2);
                        formula = "GetValue(\"LENGTH\")";
                        datatype = DOUBLE;
                        class = "Length";
                        cacheable = TRUE;
                        justify = RIGHT;
                        visibility = FALSE;
                        angle = 0;
                        length = 1;
                        decimals = 0;
                        sortdirection = ASCENDING;
                        fontname = "Arial";
                        fontcolor = 152;
                        fonttype = 2;
                        fontsize = 1;
                        fontratio = 1;
                        fontstyle = 0;
                        fontslant = 0;
                        pen = -1;
                        oncombine = NONE;
                        unit = "mm";
                    };

                    valuefield _tmp_7696
                    {
                        name = "FIELD_BOLT";
                        location = (0, 0);
                        formula = "\"( \"+Sum(\"ValueField\")+\" - \"+string(GetFieldFormula(\"name\"))+\" X \"+int(GetFieldFormula(\"length\"))+\" )\"";
                        datatype = STRING;
                        class = "";
                        cacheable = TRUE;
                        justify = CENTERED;
                        visibility = TRUE;
                        angle = 0;
                        length = 22;
                        decimals = 0;
                        sortdirection = NONE;
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

                    valuefield _tmp_10544
                    {
                        name = "name";
                        location = (45, 0);
                        formula = "GetValue(\"NAME_SHORT\")";
                        datatype = STRING;
                        class = "";
                        cacheable = TRUE;
                        justify = LEFT;
                        visibility = FALSE;
                        angle = 0;
                        length = 1;
                        sortdirection = ASCENDING;
                        fontname = "Arial";
                        fontcolor = 152;
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
    };
};
