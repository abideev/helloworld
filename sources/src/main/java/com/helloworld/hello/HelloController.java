package com.example.helloworld;

import java.util.Date;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;

@RestController

public class HelloController {
    
    @RequestMapping("/")

    public String index() {
        return "Hello World " + new Date();
    }
}
